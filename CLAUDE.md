# LMP-Link MAUI MVP - Claude Code 가이드

## 📌 프로젝트 개요

**프로젝트명**: LMP-Link MVP
**기술 스택**: .NET MAUI 10.0 (.NET 10), C# 13
**목적**: 네이버 지도 기반 반경 검색으로 장애인 활동지원 매칭 시간 단축 검증 (엑셀 30분 → 지도 3분)
**현재 진행**: Week 3 완료 ✅ / Week 4 준비 중 🟡
**전체 진행률**: 75% (21/28 days)

---

## 🎯 현재 상태 (2025-12-14 기준)

### 완료된 작업
- ✅ Week 1: 프로젝트 초기화 & 핵심 구조 (MVVM, Mock 데이터, LocationService)
- ✅ Week 2: UI & 네이버 지도 연동 (MainPage XAML, WebView JS Interop, 마커 렌더링)
- ✅ Week 3: Supabase 연동 + 관리 UX 개선 (CRUD, PersonEditPage, 선택 강조 UX)

### 진행 중인 작업
- 🟡 Week 4: 테스트 & 배포 준비

### 주요 블로커
- 없음 (빌드/실행 지침 정리 완료)

---

## 🏗️ 기술 스택 & 아키텍처

### 핵심 기술
```
.NET MAUI 10.0 (.NET 10)
├── MVVM Pattern (CommunityToolkit.Mvvm)
├── WebView + JS Interop (Naver Maps API v3)
├── Supabase (Backend)
└── DI Container (MauiProgram.cs)
```

### 폴더 구조
```
src/LmpLink.MAUI/
├── ViewModels/
│   ├── Base/BaseViewModel.cs
│   ├── MapViewModel.cs
│   ├── ManagementViewModel.cs
│   └── PersonEditViewModel.cs
├── Views/
│   ├── Pages/MainPage.xaml
│   ├── Pages/PersonEditPage.xaml
│   └── Controls/
├── Services/
│   ├── Interfaces/
│   └── Implementation/
│       ├── MockDataService.cs
│       ├── LocationService.cs
│       ├── MapService.cs
│       ├── SupabaseService.cs
│       └── ScaleService.cs
├── Models/Person.cs
├── Resources/
│   └── Raw/map.html
└── MauiProgram.cs
```

---

## 📋 핵심 원칙 & 패턴

### 1. 언어 규칙
- **코드/주석/커밋**: 영어
- **문서/UI**: 한글
- **에러 로그**: 영어

### 2. Null 안전성 (CRITICAL!)
```csharp
// ✅ 올바름
#nullable enable
if (user == null) return;

// ❌ 금지
user!.Name  // null-forgiving operator 사용 금지
```

### 3. MAUI DI 패턴 (CRITICAL!)
```csharp
// ❌ 생성자 주입 불가 (Shell DataTemplate 제약)
public MainPage(MapViewModel vm) { }

// ✅ IPlatformApplication.Current.Services 사용
public MainPage()
{
    InitializeComponent();
    var services = IPlatformApplication.Current?.Services;
    if (services == null) return;
    _viewModel = services.GetRequiredService<MapViewModel>();
}
```

### 4. XAML CommandParameter 타입 명시 (CRITICAL!)
```xml
<!-- ❌ 잘못됨: String으로 전달 -->
<Button CommandParameter="1.0" />

<!-- ✅ 올바름: 타입 명시 -->
<Button.CommandParameter>
    <x:Double>1.0</x:Double>
</Button.CommandParameter>
```

### 5. 디버깅: 파일 로깅
```csharp
// Desktop/maui_debug.log에 저장
MauiProgram.Log("message");

// 이유: dotnet run에서는 Debug.WriteLine() 출력 안 보임
```

---

## 📚 문서 참조 맵 (상황별 가이드)

### 작업 시작 전 MUST READ
1. **진척_관리.md** ⭐⭐⭐
   - 오늘 할 일 확인
   - Week별 체크리스트
   - 일일 로그 업데이트

### MVVM 작업 시
- **MAUI_개발_가이드.md** Section 4 (ViewModel 패턴)
- **Blazor_to_MAUI_참고자료.md** (검증된 로직)

### XAML/UI 작업 시
- **디자인-코드_매핑.md** ⭐ (색상/버튼/카드 1:1 매핑)
- **디자인_시스템.md** (전체 UI 원칙)

### 네이버 지도 작업 시
- **네이버_API_인증_가이드.md**
- **map.html** (Resources/Raw/map.html)

### Supabase 작업 시
- **Supabase_스키마.md** ⭐ (ERD, RLS, PostGIS)
- **Supabase_연동_가이드.md**

### 디버깅 시
- **트러블슈팅_MainPage_DI_CommandParameter.md**
- **마커_카운트_디버깅_가이드.md**

### 실행/빌드 시
- **최종_실행_가이드.md**
- **scripts/maui-build-run.ps1**
- **.cursor/rules/90-maui-run.mdc**

---

## 🚀 작업 시작 체크리스트

### 매일 작업 전 (5분)
- [ ] `진척_관리.md` 열기 → 오늘 날짜 확인
- [ ] 해당 Week의 체크리스트 확인
- [ ] 블로커 있는지 확인
- [ ] Git status 확인 (현재 브랜치: main)

### 코딩 전 (MAUI 파일 편집 시)
- [ ] Page는 기본 생성자만 사용 (`public MainPage()`)
- [ ] DI는 `IPlatformApplication.Current.Services`
- [ ] XAML CommandParameter는 타입 명시 (`<x:Double>`)
- [ ] Nullable 필드는 모든 메서드에서 null 체크
- [ ] `#nullable enable` 전체 파일 적용

### 빌드/실행 전
```powershell
# 추천: 자동화 스크립트 사용
.\scripts\maui-build-run.ps1

# 수동 실행 시
taskkill /IM LmpLink.MAUI.exe /F
dotnet build src/LmpLink.MAUI/LmpLink.MAUI.csproj
dotnet run --project src/LmpLink.MAUI/LmpLink.MAUI.csproj
```

### 작업 완료 후
- [ ] 진척_관리.md 업데이트 (완료 시간, 테스트 결과)
- [ ] 체크리스트 체크 `[x]`
- [ ] Git commit (Conventional Commits)
- [ ] 다음 작업 계획 확인

---

## 🎨 네이버 지도 API 설정 (2025년 기준)

### API 스크립트 로드 (중요!)
```html
<!-- ✅ 올바름 (2025년 신규) -->
<script src="https://oapi.map.naver.com/openapi/v3/maps.js?ncpKeyId=YOUR_CLIENT_ID"></script>

<!-- ❌ 잘못됨 (기존, 작동 안 함) -->
<script src="https://oapi.map.naver.com/openapi/v3/maps.js?ncpClientId=YOUR_CLIENT_ID"></script>
```

### 인증 키 구분
| 항목 | 용도 | 예시 |
|------|------|------|
| Client ID | JavaScript API (`ncpKeyId`) | `a5r744p216` |
| Client Secret | 서버 API 헤더용 | 긴 문자열 |

### 네이버 클라우드 플랫폼 설정
1. **Web Dynamic Map** 서비스 활성화
2. **Web 서비스 URL** 등록:
   - `http://localhost:5000`
   - `http://127.0.0.1:5000`

---

## 📊 성능 목표

| 메트릭 | 목표 | 현재 | 상태 |
|--------|------|------|------|
| 앱 시작 시간 | <2초 | 측정 필요 | 🟡 |
| 지도 로드 시간 | <3초 | 측정 필요 | 🟡 |
| 반경 필터 응답 | <500ms | 측정 필요 | 🟡 |

---

## 🚨 자주 발생하는 이슈 & 해결

### 1. MainPage 즉시 종료 문제
**원인**: CommandParameter 타입 불일치
**해결**: `트러블슈팅_MainPage_DI_CommandParameter.md` 참조

### 2. 빌드 시 exe 잠금
**해결**:
```powershell
taskkill /IM LmpLink.MAUI.exe /F
```

### 3. 선택 토글 시 플리커
**해결**: `SelectablePerson` (Observable) 사용, CheckBox `InputTransparent` 처리

### 4. Debug.WriteLine() 출력 안 보임
**해결**: `MauiProgram.Log()` 파일 로깅 사용 (Desktop/maui_debug.log)

---

## 💡 Claude Code 사용 팁

### 파일 읽기 우선순위
1. **먼저 읽어야 할 문서**:
   - 진척_관리.md (현재 상태 파악)
   - CLAUDE.md (이 문서)

2. **작업별 참조 문서**:
   - UI 작업 → 디자인-코드_매핑.md
   - DB 작업 → Supabase_스키마.md
   - 지도 작업 → 네이버_API_인증_가이드.md

### 코드 수정 시
- 기존 패턴 유지 (Early return, null 체크, DI 패턴)
- 주석은 영어로, 문서는 한글로
- 변경 전 관련 파일 먼저 읽기

### 문서 업데이트 시
- 진척_관리.md 일일 로그 업데이트 필수
- 새로운 이슈 발견 시 트러블슈팅 문서 생성
- Git commit 메시지는 Conventional Commits 형식

---

## 📝 Week 4 예정 작업 (다음 단계)

### 체크리스트
- [ ] 통합 시나리오 테스트 (지도 로드, 필터, CRUD, 편집 네비게이션)
- [ ] 성능 측정 (시작 <2s, 지도 로드 <3s, 필터 <500ms)
- [ ] Supabase 스키마/키 점검
- [ ] 실행 가이드 최종 정리
- [ ] (선택) 추천 로직 프로토타입

---

## 🔗 빠른 참조 링크

### 핵심 문서 (우선순위 순)
1. [진척_관리.md](./진척_관리.md) ⭐⭐⭐
2. [디자인-코드_매핑.md](./디자인-코드_매핑.md) ⭐⭐
3. [Supabase_스키마.md](./Supabase_스키마.md) ⭐⭐
4. [MAUI_개발_가이드.md](./MAUI_개발_가이드.md)
5. [트러블슈팅_MainPage_DI_CommandParameter.md](./트러블슈팅_MainPage_DI_CommandParameter.md)

### 설정 파일
- [MauiProgram.cs](./src/LmpLink.MAUI/MauiProgram.cs) - DI 등록
- [App.xaml.cs](./src/LmpLink.MAUI/App.xaml.cs) - 앱 초기화
- [map.html](./src/LmpLink.MAUI/Resources/Raw/map.html) - 네이버 지도

### 실행 스크립트
- [scripts/maui-build-run.ps1](./scripts/maui-build-run.ps1)

---

**버전**: 1.0
**최종 업데이트**: 2025-12-14
**작성 대상**: Claude Code AI
**프로젝트 저장소**: C:\ai\projects\map-demo
