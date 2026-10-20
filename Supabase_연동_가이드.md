# Supabase 연동 가이드

**프로젝트**: LMP-Link MVP  
**작성일**: 2025-12-10  
**목적**: Supabase 데이터베이스 연동 설정 방법

---

## 📋 진행 상황

### ✅ 완료된 작업
- [x] NuGet 패키지 설치 (`supabase-csharp` v0.16.2)
- [x] `ISupabaseService` 인터페이스 정의
- [x] `SupabaseService` 구현 (GET persons, users, assistants)
- [x] `MauiProgram.cs` DI 등록
- [x] `Person` 모델 수정 (`Experience` → `int?`)
- [x] `MockDataService` 수정 (Experience 값을 숫자로 변경)

### 🟡 대기 중 작업 (사용자 입력 필요)
- [ ] **Step 1**: Supabase 프로젝트 생성
- [ ] **Step 2**: 데이터베이스 스키마 적용
- [ ] **Step 3**: User Secrets 설정 (URL, API Key)
- [ ] **Step 4**: MapViewModel에서 Supabase 연동

---

## 🚀 Step 1: Supabase 프로젝트 생성

### 1.1 Supabase 계정 생성
1. https://supabase.com 접속
2. "Start your project" 클릭
3. GitHub 계정으로 로그인

### 1.2 새 프로젝트 생성
1. 좌측 사이드바 → "New Project" 클릭
2. 프로젝트 정보 입력:
   - **Name**: `lmplink-mvp` (또는 원하는 이름)
   - **Database Password**: 강력한 비밀번호 생성 (저장 필수!)
   - **Region**: `Northeast Asia (Seoul)` 선택
   - **Pricing plan**: Free tier 선택
3. "Create new project" 클릭
4. 프로젝트 생성 대기 (약 2분)

### 1.3 프로젝트 URL 및 API Key 확인
1. 프로젝트 대시보드 → Settings → API
2. 다음 정보 복사:
   - **Project URL**: `https://your-project.supabase.co`
   - **anon (public) key**: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`

---

## 🗄️ Step 2: 데이터베이스 스키마 적용

### 2.1 SQL Editor 열기
1. Supabase 대시보드 → SQL Editor
2. "New query" 클릭

### 2.2 스키마 생성 쿼리 실행

아래 SQL을 복사하여 실행:

```sql
-- ========================================
-- LMP-Link MVP Database Schema
-- ========================================

-- Enable PostGIS extension for geographic data
CREATE EXTENSION IF NOT EXISTS postgis;

-- ========================================
-- Table: persons (이용자 + 지원사 통합)
-- ========================================
CREATE TABLE persons (
  id SERIAL PRIMARY KEY,
  
  -- Basic info
  name VARCHAR(100) NOT NULL,
  type VARCHAR(20) NOT NULL CHECK (type IN ('user', 'assistant')),
  email VARCHAR(255) UNIQUE NOT NULL,
  phone VARCHAR(20),
  
  -- Address & Location
  address TEXT NOT NULL,
  latitude DOUBLE PRECISION NOT NULL,
  longitude DOUBLE PRECISION NOT NULL,
  
  -- Profile (nullable, some fields for assistants only)
  gender VARCHAR(10) CHECK (gender IN ('male', 'female', 'other', NULL)),
  has_vehicle BOOLEAN DEFAULT false,
  available_time_slots VARCHAR(50), -- e.g. "weekday_am", "weekend"
  experience_years INTEGER,
  
  -- Metadata
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Create indexes for performance
CREATE INDEX idx_persons_type ON persons (type);
CREATE INDEX idx_persons_email ON persons (email);

-- ========================================
-- Insert Sample Data (Mock Data)
-- ========================================

-- Users (10명)
INSERT INTO persons (id, name, type, email, phone, address, latitude, longitude, gender, has_vehicle, available_time_slots, experience_years)
VALUES
(1, '김영희', 'user', 'user1@example.com', '010-1234-5001', '서울 도봉구 도봉동 123', 37.6738, 127.0501, 'female', false, NULL, NULL),
(2, '이철수', 'user', 'user2@example.com', '010-1234-5002', '서울 도봉구 쌍문동 234', 37.6608, 127.0531, 'male', false, NULL, NULL),
(3, '박지영', 'user', 'user3@example.com', '010-1234-5003', '서울 도봉구 방학동 345', 37.6796, 127.0431, 'female', false, NULL, NULL),
(4, '최민수', 'user', 'user4@example.com', '010-1234-5004', '서울 도봉구 창동 456', 37.6538, 127.0391, 'male', false, NULL, NULL),
(5, '정수진', 'user', 'user5@example.com', '010-1234-5005', '서울 강북구 미아동 567', 37.6850, 127.0571, 'female', false, NULL, NULL),
(6, '한동훈', 'user', 'user6@example.com', '010-1234-5006', '서울 강북구 번동 678', 37.6488, 127.0591, 'male', false, NULL, NULL),
(7, '윤서연', 'user', 'user7@example.com', '010-1234-5007', '서울 노원구 상계동 789', 37.6938, 127.0321, 'female', false, NULL, NULL),
(8, '강태오', 'user', 'user8@example.com', '010-1234-5008', '서울 노원구 중계동 890', 37.6368, 127.0291, 'male', false, NULL, NULL),
(9, '임은지', 'user', 'user9@example.com', '010-1234-5009', '서울 성북구 장위동 901', 37.7008, 127.0671, 'female', false, NULL, NULL),
(10, '조현우', 'user', 'user10@example.com', '010-1234-5010', '서울 성북구 석관동 012', 37.6338, 127.0691, 'male', false, NULL, NULL);

-- Assistants (20명)
INSERT INTO persons (id, name, type, email, phone, address, latitude, longitude, gender, has_vehicle, available_time_slots, experience_years)
VALUES
-- 반경 0-1km (가까운 거리)
(11, '김지원', 'assistant', 'assistant1@example.com', '010-2001-0001', '서울 도봉구 도봉동 111-1', 37.6718, 127.0491, 'female', true, 'weekday_am', 5),
(12, '이민호', 'assistant', 'assistant2@example.com', '010-2001-0002', '서울 도봉구 도봉동 111-2', 37.6648, 127.0501, 'male', true, 'weekday_pm', 3),
(13, '박서현', 'assistant', 'assistant3@example.com', '010-2001-0003', '서울 도봉구 쌍문동 222-1', 37.6733, 127.0451, 'female', false, 'weekend', 2),
(14, '최우진', 'assistant', 'assistant4@example.com', '010-2001-0004', '서울 도봉구 쌍문동 222-2', 37.6628, 127.0431, 'male', true, 'weekday_am', 4),
(15, '정다은', 'assistant', 'assistant5@example.com', '010-2001-0005', '서울 도봉구 방학동 333-1', 37.6751, 127.0521, 'female', true, 'weekday_pm', 6),

-- 반경 1-2km (중간 거리)
(16, '한지훈', 'assistant', 'assistant6@example.com', '010-2001-0006', '서울 도봉구 방학동 333-2', 37.6588, 127.0551, 'male', false, 'weekend', 1),
(17, '윤채원', 'assistant', 'assistant7@example.com', '010-2001-0007', '서울 도봉구 창동 444-1', 37.6796, 127.0381, 'female', true, 'weekday_am', 5),
(18, '강민석', 'assistant', 'assistant8@example.com', '010-2001-0008', '서울 도봉구 창동 444-2', 37.6558, 127.0371, 'male', true, 'weekday_pm', 4),
(19, '임소라', 'assistant', 'assistant9@example.com', '010-2001-0009', '서울 강북구 미아동 555-1', 37.6823, 127.0581, 'female', false, 'weekend', 7),
(20, '조태호', 'assistant', 'assistant10@example.com', '010-2001-0010', '서울 강북구 미아동 555-2', 37.6528, 127.0591, 'male', true, 'weekday_am', 3),

-- 반경 2-3km
(21, '신예린', 'assistant', 'assistant11@example.com', '010-2001-0011', '서울 강북구 번동 666-1', 37.6868, 127.0331, 'female', true, 'weekday_pm', 8),
(22, '배준영', 'assistant', 'assistant12@example.com', '010-2001-0012', '서울 강북구 번동 666-2', 37.6478, 127.0321, 'male', false, 'weekend', 6),
(23, '서유진', 'assistant', 'assistant13@example.com', '010-2001-0013', '서울 노원구 상계동 777-1', 37.6919, 127.0631, 'female', true, 'weekday_am', 5),
(24, '오현수', 'assistant', 'assistant14@example.com', '010-2001-0014', '서울 노원구 상계동 777-2', 37.6448, 127.0641, 'male', true, 'weekday_pm', 2),
(25, '권나영', 'assistant', 'assistant15@example.com', '010-2001-0015', '서울 노원구 중계동 888-1', 37.6948, 127.0291, 'female', false, 'weekend', 9),

-- 반경 3-4km (먼 거리)
(26, '송재민', 'assistant', 'assistant16@example.com', '010-2001-0016', '서울 노원구 중계동 888-2', 37.6408, 127.0271, 'male', true, 'weekday_am', 4),
(27, '홍수아', 'assistant', 'assistant17@example.com', '010-2001-0017', '서울 성북구 장위동 999-1', 37.6988, 127.0681, 'female', true, 'weekday_pm', 10),
(28, '노지훈', 'assistant', 'assistant18@example.com', '010-2001-0018', '서울 성북구 장위동 999-2', 37.6368, 127.0701, 'male', false, 'weekend', 7),
(29, '황지원', 'assistant', 'assistant19@example.com', '010-2001-0019', '서울 성북구 석관동 000-1', 37.7028, 127.0231, 'female', true, 'weekday_am', 3),
(30, '안태양', 'assistant', 'assistant20@example.com', '010-2001-0020', '서울 성북구 석관동 000-2', 37.6328, 127.0211, 'male', true, 'weekday_pm', 12);

-- Reset sequence to continue from 31
SELECT setval('persons_id_seq', 30, true);

-- ========================================
-- RLS (Row Level Security) - Optional for MVP
-- ========================================
-- Disable RLS for MVP (enable later for production)
-- ALTER TABLE persons ENABLE ROW LEVEL SECURITY;

-- ========================================
-- Test Query
-- ========================================
-- Verify data insertion
SELECT 
    type,
    COUNT(*) as count
FROM persons
GROUP BY type;

-- Expected output:
-- type      | count
-- ----------|-------
-- user      | 10
-- assistant | 20
```

### 2.3 쿼리 실행 확인
1. "Run" 버튼 클릭
2. 결과 확인:
   - `user`: 10명
   - `assistant`: 20명

---

## 🔐 Step 3: User Secrets 설정

### 3.1 User Secrets 초기화 (이미 완료됨)
.csproj 파일에 `<UserSecretsId>`가 이미 설정되어 있습니다:
```xml
<UserSecretsId>97633f9d-934c-4b32-97de-c21df1d745fb</UserSecretsId>
```

### 3.2 Supabase URL 및 API Key 저장
터미널에서 실행:

```bash
# 프로젝트 디렉토리로 이동
cd C:\ai\projects\map-demo\src\LmpLink.MAUI

# Supabase URL 설정 (your-project를 실제 프로젝트 ID로 변경)
dotnet user-secrets set "SupabaseUrl" "https://your-project.supabase.co"

# Supabase API Key 설정 (your-anon-key를 실제 anon key로 변경)
dotnet user-secrets set "SupabaseKey" "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### 3.3 SupabaseService 수정 (User Secrets 사용)
`src/LmpLink.MAUI/Services/Implementation/SupabaseService.cs` 파일의 생성자를 수정:

```csharp
public SupabaseService(IConfiguration configuration)
{
    _supabaseUrl = configuration["SupabaseUrl"] ?? throw new InvalidOperationException("SupabaseUrl not configured");
    _supabaseKey = configuration["SupabaseKey"] ?? throw new InvalidOperationException("SupabaseKey not configured");
}
```

그리고 `MauiProgram.cs`에서 Configuration 설정:

```csharp
builder.Configuration.AddUserSecrets<MauiProgram>();
```

---

## 🔗 Step 4: MapViewModel에서 Supabase 연동

### 4.1 MapViewModel에 ISupabaseService 주입
`src/LmpLink.MAUI/ViewModels/MapViewModel.cs` 수정:

```csharp
public partial class MapViewModel : BaseViewModel
{
    private readonly IMockDataService _mockDataService;
    private readonly ILocationService _locationService;
    private readonly ISupabaseService _supabaseService; // 추가

    public MapViewModel(
        IMockDataService mockDataService,
        ILocationService locationService,
        ISupabaseService supabaseService) // 추가
    {
        _mockDataService = mockDataService;
        _locationService = locationService;
        _supabaseService = supabaseService;
    }

    // LoadDataCommand 수정
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            MauiProgram.Log("[MapViewModel] LoadDataAsync START");

            try
            {
                // Option 1: Supabase 연동
                await _supabaseService.InitializeAsync();
                var users = await _supabaseService.GetUsersAsync();
                var assistants = await _supabaseService.GetAssistantsAsync();

                MauiProgram.Log($"[MapViewModel] Loaded from Supabase: {users.Count} users, {assistants.Count} assistants");
            }
            catch (Exception ex)
            {
                MauiProgram.Log($"[MapViewModel] Supabase failed: {ex.Message}. Falling back to Mock data.");

                // Option 2: Mock Data Fallback
                var users = await _mockDataService.GetUsersAsync();
                var assistants = await _mockDataService.GetAssistantsAsync();

                MauiProgram.Log($"[MapViewModel] Loaded from Mock: {users.Count} users, {assistants.Count} assistants");
            }

            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }

            Assistants.Clear();
            foreach (var assistant in assistants)
            {
                Assistants.Add(assistant);
            }

            MauiProgram.Log("[MapViewModel] LoadDataAsync END");
        });
    }
}
```

---

## 🧪 Step 5: 테스트

### 5.1 빌드 및 실행
```bash
dotnet build C:\ai\projects\map-demo\src\LmpLink.MAUI\LmpLink.MAUI.csproj
dotnet run --project C:\ai\projects\map-demo\src\LmpLink.MAUI\LmpLink.MAUI.csproj
```

### 5.2 로그 확인
`Desktop/maui_debug.log` 파일에서 확인:
- `[SupabaseService] Initializing...`
- `[SupabaseService] Fetched X users`
- `[SupabaseService] Fetched Y assistants`

### 5.3 Fallback 테스트
Supabase URL을 잘못된 값으로 설정하고 Mock 데이터로 Fallback되는지 확인:
```bash
dotnet user-secrets set "SupabaseUrl" "https://invalid-url.supabase.co"
```

---

## 📊 완료 기준

- ✅ Supabase 프로젝트 생성 완료
- ✅ `persons` 테이블에 30명 데이터 삽입 완료
- ✅ User Secrets 설정 완료
- ✅ MapViewModel에서 Supabase 데이터 로드 성공
- ✅ 네트워크 오류 시 Mock 데이터 Fallback 동작 확인

---

## 🔗 참고 문서

- [Supabase 스키마 정의](./Supabase_스키마.md)
- [MAUI 개발 가이드](./MAUI_개발_가이드.md)
- [작업 일관성 분석](./작업_일관성_분석_및_통합_계획.md)

---

**작성일**: 2025-12-10  
**버전**: 1.0  
**다음 업데이트**: Supabase 연동 완료 후














