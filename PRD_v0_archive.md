# PRD v0 (Archive)

> **Note:** This is the original PRD archived on 2025-12-08. The current PRD has been rewritten based on the detailed development roadmap and design system.

---

## Product Overview

장애인 활동지원 위치기반 스마트 매칭 시스템(LMP-Link)은 중개기관 코디네이터가 이용자와 활동지원사를 지도 기반으로 직관적으로 탐색하고, 반경 검색과 AI 분석을 통해 최적 후보를 빠르게 추천받을 수 있도록 하는 Windows/Android 하이브리드 애플리케이션입니다. Blazor Hybrid(.NET 9)와 Supabase(PostgreSQL + PostGIS)를 기반으로 하는 BaaS/Serverless 아키텍처를採用하여, 복잡한 서버 구축 없이도 실시간 매칭, 반경 검색, AI 추천 기능을 제공하는 것이 핵심입니다.

## Purpose

본 시스템의 1차 목적은 엑셀·수기 기반으로 운영되던 활동지원 매칭 프로세스를 완전히 디지털화하여, 코디네이터의 매칭 업무 시간을 50% 이상 단축하고, 활동지원사의 이동 거리와 이용자-지원사 간 미스매치를 줄이는 것입니다.  
구체적으로, 지도 기반의 위치 시각화, PostGIS 기반 반경 검색, n8n + LLM 기반 성향/이력 텍스트 분석을 결합해 "근거리 + 조건 적합 + 성향 적합"을 동시에 만족하는 후보를 제안하는 실무 도구를 제공하는 것을 목표로 합니다.

## Target Audience

주요 대상은 중개기관의 전담 코디네이터 및 실무 담당자입니다. 이들은 Windows 환경에서 데스크톱 앱을 사용하며, 기존에는 엑셀과 전화, 메신저를 병행해 비효율적으로 매칭을 수행하고 있습니다.  
2차 대상은 활동지원사로, 주로 Android 스마트폰을 통해 자신의 배정 현황, 가매칭 알림 등을 확인합니다. 이들에게는 복잡한 업무 화면보다 간단한 매칭 알림 및 기본 정보 확인 UI가 필요합니다.

## Expected Outcomes

단기적으로는 매칭 속도를 건당 30분에서 3분 이내로 단축하고, 수기 입력 과정에서 발생하는 데이터 오류를 95% 이상 줄이는 것을 목표로 합니다. 중기적으로는 근거리 우선 매칭을 통해 활동지원사의 이동 거리를 평균 20% 이상 감소시키고, 이용자·지원사의 만족도(재배정 비율, 클레임 건수 등)를 개선하는 것이 기대 효과입니다.  
KPI로는 매칭 리드타임, 매칭 실패/재매칭률, 평균 이동 거리, 코디네이터 1인당 일일 처리 건수 등을 주요 지표로 사용합니다.

## Design Details

LMP-Link는 Blazor Hybrid 기반 클라이언트가 Supabase를 통해 직접 데이터에 접근하는 BaaS 중심 구조입니다. Windows/Android/Web UI는 모두 동일한 .NET 9 코드베이스를 공유하며, 위치 데이터는 PostGIS Geography 타입으로 관리됩니다. 지도는 Naver Maps JavaScript SDK를 Blazor WebView에서 호출하는 방식으로 통합됩니다.  
데이터 파이프라인은 엑셀 업로드 후 n8n에서 주소→좌표 변환을 비동기로 처리해 Supabase에 저장합니다. 매칭 로직은 Supabase RPC 및 Edge Functions로 캡슐화하고, AI 매칭 분석은 n8n이 Webhook 트리거를 받아 LLM을 호출한 후 결과를 DB에 저장, Supabase Realtime 채널로 클라이언트에 전파하는 패턴입니다.

## Architectural Overview

전체 아키텍처는 다음과 같은 구성 요소로 이뤄집니다.

- 클라이언트 레이어: Blazor Hybrid 앱 (Windows, Android, Web). MudBlazor를 사용해 지도 패널, 리스트 패널, 상세 패널을 구성합니다.  
- BaaS 레이어: Supabase (PostgreSQL + PostGIS, Auth, Realtime, Storage, Edge Functions, RPC). 클라이언트는 Supabase .NET SDK를 통해 인증/CRUD/RPC 호출을 수행합니다.  
- 오케스트레이션/AI 레이어: n8n 워크플로우 서버. 엑셀 파일 업로드 Webhook, Naver Geocoding 호출, LLM(Claude 3.5) 호출, 결과를 Supabase에 저장하는 기능을 담당합니다.  
- 외부 서비스: Naver Maps API v3(지도, Marker Clustering, Geocoding), Claude 3.5(LLM).

컴포넌트 간 주요 통신은 HTTPS 기반 REST/RPC 호출이며, 실시간 업데이트는 Supabase Realtime 채널(WebSocket)을 사용합니다. 복잡한 비즈니스 로직은 클라이언트 코드에 과도하게 쌓이지 않도록 RPC 및 Edge Function에서 처리합니다.

## Data Structures and Algorithms

데이터 구조는 다음 네 개를 중심으로 설계합니다.

- profiles: 사용자 기본 정보(이용자/지원사), 역할, 연락처, 메타데이터.  
- locations: 사용자별 최신 위치 정보, Geography(Point) 및 원본 주소.  
- preferences: JSONB로 저장되는 선호/기피 조건, 근무 시간대, 성별 등 필터링 조건.  
- matches: 매칭 히스토리와 상태(가매칭, 확정, 취소 등), AI 추천 점수 및 사유 메타데이터.

반경 검색 알고리즘은 PostGIS의 ST_DWithin을 이용하여, 기준 좌표와 지정 반경(1km/3km/5km)에 포함되는 활동지원사를 필터링합니다. 인덱스는 Geography 컬럼에 GiST 인덱스를 생성하여 성능을 확보합니다.  
AI 매칭 알고리즘은 이용자의 요구사항 텍스트와 활동지원사 이력 텍스트를 n8n에서 LLM에 전달하고, LLM으로부터 추천 점수 및 설명(추천 사유)을 JSON 형태로 반환받아 matches 또는 별도 recommendation_result 테이블에 저장합니다. 이후 앱에서 점수 상위 3명을 정렬 후 카드 형태로 노출합니다.

## System Interfaces

주요 인터페이스는 다음과 같습니다.

- Blazor ↔ Supabase:  
  - Auth API: 이메일/비밀번호 로그인, 역할 기반 세션.  
  - Database CRUD: profiles, locations, preferences, matches에 대한 Select/Insert/Update.  
  - RPC: get_nearby_assistants(user_id, radius_km) 등 반경 검색용 Stored Procedure.  
  - Realtime: matches 및 ai_recommendations 채널 구독.  
- Blazor ↔ Naver Maps:  
  - JavaScript interop을 통한 지도 초기화, Marker/Cluster, Circle Overlay 렌더링, 클릭 이벤트 핸들링.  
- Blazor ↔ n8n:  
  - AI 분석 요청 Webhook 호출 (user_id, candidate_ids, 설명 텍스트 등 payload).  
- n8n ↔ Supabase:  
  - 엑셀 업로드 처리, Geocoding 결과 저장, AI 분석 결과 저장.

각 인터페이스는 HTTPS/JSON 기반이며, 인증이 필요한 Supabase 접근은 Supabase Auth 토큰을, n8n Webhook은 전용 시크릿 토큰 또는 IP 허용 목록으로 보호합니다.

## User Interfaces

Windows용 UI는 좌측 70% 지도, 우측 30% 리스트/상세 패널을 가지는 Split View 레이아웃입니다. MudBlazor의 AppBar, Drawer, DataGrid, Dialog를 활용하여 필터 패널, 지원사 리스트, 매칭 상세 모달을 구성합니다.  
지도 인터랙션은 이용자 마커 클릭 → 기본 3km 반경 표시 → 반경 내 지원사 하이라이팅 및 리스트 필터링 흐름을 기본으로 합니다. PC 환경에서는 지원사 마커를 이용자 마커로 드래그해 "가매칭" 상태로 저장하는 Drag & Drop 인터랙션을 제공하며, Drag 완료 시 matches에 Draft 상태 레코드를 생성합니다.  
모바일(Android) UI는 풀스크린 지도와 하단 Bottom Sheet 구조로, 지도 상의 마커 선택 시 Bottom Sheet에서 상세 정보와 매칭 상태를 표시합니다. 접근성을 위해 이용자와 지원사를 색상뿐 아니라 형태(원형/사각형)로 구분합니다.

## Hardware Interfaces

주요 하드웨어 의존성은 없습니다. Windows PC에서는 기본적인 네트워크 연결 및 마우스/키보드 입력, Android 디바이스에서는 터치 입력과 인터넷 연결만을 요구합니다.  
추후 활동지원사 단말의 GPS를 활용한 실시간 위치 업데이트는 고려 대상이지만, 이번 버전에서는 정적 주소 기반 좌표만을 사용하며, 모바일 단말의 위치 센서는 필수 요건이 아닙니다.

## Testing Plan

테스트 전략은 1인 개발자가 빠르게 커버할 수 있도록 자동화 비중을 높이되, 핵심 유스케이스(반경 검색, 엑셀 업로드, AI 추천)를 중심으로 한 시나리오 기반 수동 테스트를 병행하는 방식입니다.  
Unit Test로 Supabase RPC/Edge Function 로직과 클라이언트 로컬 서비스 클래스를 검증하고, Integration Test 수준으로 Dev 스키마에 대한 실제 쿼리와 Realtime 동작을 확인합니다. UI는 핵심 플로우에 대해 수동 검사와 간단한 스냅샷/상태 테스트를 적용합니다.

## Test Strategies

핵심 전략은 다음 네 가지입니다.

- 유닛 테스트: 반경 계산 파라미터, 매칭 후보 필터링(시간대, 성별, 기피 대상 등)을 클라이언트 로직 관점에서 검증합니다.  
- 통합 테스트: Dev 스키마에 대해 get_nearby_assistants RPC, RLS 정책, Realtime 구독 흐름을 실제로 호출해 본 뒤, 기대 레코드만 접근 가능한지 확인합니다.  
- UI/UX 테스트: 지도 렌더링, 마커 클러스터링 성능, Drag & Drop 매칭, 모바일 Bottom Sheet 동작 등 주요 UX 패턴을 수동 검증합니다.  
- 회귀 테스트: 엑셀 포맷 변경, Naver API 에러, n8n 장애 등 외부 요인 변경 시 주요 기능이 정상 동작하는지 간단한 체크리스트 기반으로 확인합니다.

## Testing Tools

개발 및 테스트 도구는 다음과 같이 구성합니다.

- .NET 테스트: xUnit 또는 NUnit 기반 자동화 테스트 프로젝트.  
- DB/쿼리 검증: Supabase SQL Editor, DBeaver 등 PostgreSQL 클라이언트.  
- API/Flow 검증: Postman 또는 Insomnia로 RPC, Edge Function, n8n Webhook 호출 검증.  
- UI 테스트: Browser DevTools(Blazor WebView 내부) 및 Naver Maps Debug 툴, 간단한 스크린 레코딩 도구를 활용해 UX 동작을 캡처합니다.

## Testing Environments

테스트는 단일 Supabase 프로젝트 내 Dev 스키마에서 수행합니다. Dev 스키마는 테스트용 계정/더미 데이터를 사용하며, RLS 정책을 Prod와 동일하게 적용해 동작을 검증합니다.  
클라이언트 측에서는 Dev 스키마에 연결된 별도 Supabase URL/Key를 사용하며, Windows/Android 빌드는 "Dev" 설정을 통해 명시적으로 구분합니다. 성능 검증은 Dev 환경에 대량 더미 데이터를 주입하여 지도 마커 1,000개 이상일 때의 렌더링 및 반응성을 측정하는 방식으로 진행합니다.

## Test Cases

중요 테스트 케이스는 다음과 같습니다.

- 엑셀 업로드 시나리오: 정상 파일 업로드 → n8n 지오코딩 완료 → locations에 올바른 좌표 및 주소 저장 여부. 주소 오류/누락 시 오류 처리 동작.  
- 반경 검색: 특정 이용자 선택 후 1/3/5km 반경별 지원사 수, 거리 순 정렬 검증.  
- 필터 조합: 시간대, 자차 여부, 성별, 기피 대상 필터를 AND/OR 조합했을 때 기대 후보군만 리스트에 남는지 확인.  
- AI 분석: 후보군 선택 후 AI 분석 요청 → 일정 시간 후 Realtime을 통해 추천 결과 수신 및 상위 3명 카드 표시 여부.  
- Drag & Drop 가매칭: 마커를 드래그해 Dropping 시 Draft 매칭이 생성되고, 재실행 시 상태 변경이 올바르게 반영되는지 확인.

## Reporting and Metrics

테스트 결과는 간단한 테스트 시트(스프레드시트)와 이슈 트래커(예: GitHub Issues)로 관리합니다. 각 테스트 케이스는 상태(Pass/Fail), 재현 단계, 실제·기대 결과를 기록합니다.  
품질 메트릭으로는 릴리즈 전 오픈된 Critical 버그 수, 주요 사용자 플로우(지도 로딩, 반경 검색, AI 추천) 성공률, 첫 주 사용자 피드백에서의 UX 장애 건수를 추적합니다.

## Deployment Plan

배포는 초기에는 스토어 심사를 우회하기 위해 사이드로드 방식으로 진행합니다. 빌드는 Dev/Prod 설정을 분리하되, 실제 운영 배포는 Prod 스키마 접속을 기본으로 합니다.  
배포의 기본 흐름은 Git 저장소 태깅 → CI 빌드 아티팩트 생성 → Windows MSIX 패키지 및 Android APK 생성 → 내부 배포 채널(직접 다운로드 또는 내부 공유)에 업로드 → 코디네이터 대상 설치 가이드 제공입니다.

## Deployment Environment

운영 환경은 Supabase 단일 프로젝트 내 Prod 스키마를 사용하며, 기본적인 고가용성과 백업은 Supabase에서 제공하는 기능에 의존합니다. 데이터베이스는 Postgres + PostGIS로 구성되며, RLS 정책을 통해 계정별 접근을 제한합니다.  
클라이언트 배포 대상은 Windows 10 이상 PC 및 Android 9 이상 스마트폰을 가정합니다. 네트워크는 일반 인터넷 환경에서 HTTPS 통신이 가능하면 되며, 별도 온프레미스 서버는 사용하지 않습니다.

## Deployment Tools

빌드 및 배포 도구는 다음과 같이 구성합니다.

- .NET SDK 9를 이용한 Blazor Hybrid 빌드 툴체인.  
- Windows: MSIX Packaging Tool 또는 .NET publish 옵션을 통한 MSIX 생성.  
- Android: .NET for Android 빌드로 APK 생성, 서명 키 관리.  
- CI: GitHub Actions 또는 유사 CI 서비스에서 빌드/테스트 자동화(초기에는 수동 빌드 후 점진적 도입 가능).

## Deployment Steps

대표적인 배포 단계는 다음과 같습니다.

1. main 브랜치에 변경사항 머지 및 버전 태깅.  
2. CI 또는 로컬 환경에서 Release 구성으로 빌드 및 테스트 실행.  
3. MSIX, APK 아티팩트 생성 및 버전/체크섬 기록.  
4. 내부 공유 경로(사내 포털, 공유 드라이브 등)에 아티팩트 업로드.  
5. 코디네이터 및 활동지원사 대상 설치 가이드(스크린샷 포함) 배포.  
6. 배포 후 첫 사용 시 오류/장애 여부 모니터링.  
롤백이 필요한 경우, 이전 버전 MSIX/APK 재배포 및 Supabase 스키마 마이그레이션 롤백(필요 시)으로 처리합니다.

## Post-Deployment Verification

배포 직후에는 다음 사항을 집중 점검합니다. 지도 초기 로딩 성능, 로그인 및 RLS 기반 데이터 접근 정상 여부, 반경 검색 결과의 정확도, 엑셀 업로드 파이프라인 정상 동작, AI 추천 요청 및 결과 수신 여부입니다.  
이슈가 발견되면 영향 범위와 심각도를 평가해 핫픽스 배포 여부를 결정하고, 데이터 손상이 의심되는 경우 Supabase 백업 시점 기준으로 복구 가능성을 검토합니다.

## Continuous Deployment

초기 단계에서는 완전한 자동 배포까지는 가지 않되, CI 상에서 테스트와 빌드까지는 자동화하고, 아티팩트 배포와 설치 안내는 수동으로 진행합니다. 이후 안정화 단계에서 Git 태그를 트리거로 MSIX/APK를 자동 업로드하는 스크립트를 추가해 반자동 Continuous Deployment에 가깝게 발전시킬 수 있습니다.  
이 접근은 1인 개발 환경에서 무리하지 않는 선에서 릴리즈 품질을 유지하면서도, 빈번한 기능 개선과 버그 수정을 빠르게 사용자에게 전달하기 위한 균형을 맞추기 위함입니다.

---





















