# Firebase LiveOps v3.1

Unity 2D 모바일 방치형 RPG 프로토타입

## 기본 정보

- Unity: `6000.4.8f1`
- Firebase Unity SDK: `13.11.0`
- Android Bundle ID: `com.DoOurGame.gameliveops`
- 현재 목표 플랫폼: Android
- 현재 로그인 범위: Google 로그인 + 게스트 로그인
- 주요 기술: Firebase Authentication, Firestore, Analytics, Remote Config, Crashlytics, Messaging, Unity IAP, 광고 보상 구조

## 포함 기능

### 세계관 / 스토리

- 조류와 고양이가 각 동네의 전봇대를 두고 전쟁 중인 세계관
- 플레이어는 전쟁 중 왼쪽 날개가 부러진 참새 이등병
- 플레이어는 직접 전방에서 싸우기보다 뒤에서 전력을 충전해 동료 스킬 사용을 지원
- 신규 유저 첫 진입 시 만화 컷 형식의 메인 스토리 튜토리얼 표시
- 튜토리얼 스토리는 화면 전체를 컷신 이미지로 채우고, 별도 텍스트 박스 없이 진행
- 현재 1~3번 컷은 제공된 이미지 기반, 4~7번 컷은 임시 컷신 이미지로 구성
- 화면 클릭/다음 버튼으로 컷을 넘기고, 이전 버튼으로 놓친 컷을 다시 확인 가능
- 귀엽고 둥근 픽셀 아트 스타일 기준

### 계정 / Firebase

- Firebase 초기화 및 연결 상태 확인
- 게스트 자동 로그인
- Google 계정 연동용 credential 구조
- Android Google 로그인 전용 UI 및 상태 진단
- 계정 상태, Firebase Ready, Remote Config, FCM 토큰 상태 표시
- Firestore 기반 플레이어 데이터 저장/로드
- Firebase Analytics 로그인/뽑기/SSR 획득 이벤트 기록
- Firebase Remote Config 기반 뽑기 확률 적용
- Firebase Crashlytics, Messaging 패키지 연동 기반 준비
- FCM 토큰 획득 및 저장

### 플레이어 데이터 / 저장

- `PlayerData` 기본 구조
- 기존 데이터 호환을 위한 `PlayerDataConverter`
- 골드, 젬, 뽑기 티켓, 인벤토리 데이터
- 마지막 접속 시간 저장
- 방치 보상 계산 및 지급
- 자동 저장 및 앱 종료/일시정지 시 저장 처리
- 테스트 편의를 위한 기본 골드/젬 100,000 지급

### 전투 / 진행

- 세로형 메인 전투 화면
- 자동 전투 기반 전투 루프
- 스테이지 진행
- 자동 진행 ON/OFF
- 적 체력/플레이어 체력 표시
- 보스/일반 몬스터 구조 확장 준비
- 보스 패턴 데이터베이스 기초
- 참새 이등병은 직접 공격하지 않고 전력 충전 무기로 동료를 지원
- 유저 탭으로 전력 게이지를 채우는 전력 충전 버튼
- 장착 동료 3명이 고양이 적을 자동 공격
- 동료 스킬은 쿨다운과 전력 조건을 만족하면 사용 가능
- 전투 시각화용 임시 카드/UI
- 추후 실제 스프라이트 교체 가능 구조
- 참새 이등병, 동료 3명, 고양이 적의 전투 위치를 prefab의 `ActorRoot` 기준으로 조정 가능
- 동료 배치는 캐릭터 1 위쪽, 캐릭터 2 중앙, 캐릭터 3 아래쪽으로 고정
- 스테이지당 고양이 적 1마리 표시
- 맵별 발판에 맞춰 `EnemyActorRoot`, `SupportActorRoot`, `CompanionActorRoot1~3` 위치를 직접 조정 가능
- 전투 무대는 `BattlefieldLayer` 안의 `BattlefieldGuideLayer`, `BattlefieldActorLayer`, `BattlefieldEffectLayer`로 분리
- 전투 캐릭터는 `ActorRoot > Shadow + Visual + DamageAnchor + HealthAnchor` 구조로 배치
- 실제 전투 아트는 `CharacterData.battleVisual`과 `BattleVisualDatabase`에서 sprite, Animator, 프레임 애니메이션, 투사체를 교체
- 실제 아트 파일은 `Assets/Art/Battle` 아래에 넣고 `Tools > Battle Art > Auto Link Battle Visuals`로 자동 연결
- `BattleLayoutConfig`의 전투 좌표는 prefab 생성/fallback용 발 위치 기준으로 사용
- 임시 `ActorShadow` 스프라이트로 캐릭터 발밑 그림자 표시
- 캐릭터 draw order는 전투 y좌표 기준으로 정렬하여 아래쪽 캐릭터가 앞에 보이도록 처리
- 동료 공격용 투사체 구조와 임시 전기 투사체 리소스
- 대상 머리 위에 표시되는 데미지 숫자 UI
- 1,000 이상 숫자를 A/B/C 방식으로 줄여 보여주는 `CompactNumberFormatter`
- 고양이 적 픽셀 아트 및 애니메이션 에셋 교체 준비 구조
- 전투 화면은 상단 2/3 전투 무대, 하단 1/3 전력 충전/캐릭터 스킬 컨트롤 패널 구조
- `BattleHud.prefab` 기준 전투 화면을 직접 조정할 수 있도록 개별 전투 프리뷰 씬과 보정 메뉴 추가

### 성장 / 장비

- 공격력, 체력, 공격 속도 성장 업그레이드
- 골드 비용 기반 업그레이드
- 장비 데이터 구조
- 무기/장비 장착 및 강화 구조
- 장비 보유 효과/장착 효과 확장 준비
- 장비 상세 UI 기초

### 캐릭터 / 동료

- ScriptableObject 기반 캐릭터 데이터베이스
- 임시 캐릭터 이름/등급 데이터
- 캐릭터 도감 UI
- 캐릭터 상세 정보 UI
- 직접 선택 후 장착/해제 가능한 구조
- 동료 여러 명 장착을 위한 슬롯 구조
- 동료 슬롯 3칸과 빈 슬롯 `+` 표시
- 미보유 캐릭터는 이름 대신 `?`와 자물쇠 아이콘으로 표시
- 동료 특성/시너지 시스템 기초
- 추후 실제 동료 스프라이트 연결 가능 구조
- 테스트용 SR 까치 `Jack`, SSR 뱁새 `Xenon` 캐릭터 데이터 및 전투 아트 연결

### 뽑기

- 동료 뽑기 화면
- 1회/10회 뽑기
- 티켓 우선 사용 후 젬 사용
- SSR/SR/R 등급 확률
- 10회 뽑기 SR 이상 보장
- 100회 천장 SSR 보장
- 뽑기 결과 표시
- 뽑기 배너/연출/상세 확률 UI 확장 준비

### 퀘스트 / 라이브옵스

- 메인 퀘스트 순환 구조
- 일일 퀘스트가 아니라 하나를 완료하면 다음 퀘스트가 열리는 방식
- 현재 순환: 몬스터 처치, 전력 충전, 동료 모집, 영웅 강화, 장비 강화
- 일일 보상 구조
- 우편함 생성, 개별 수령, 전체 수령
- Firestore `global_mails` 기반 운영 우편 조회
- 이벤트 미션 시스템 기초
- 기간 이벤트/출석/룰렛/패스 확장 준비

### 상점 / 광고 / 결제

- 상점 UI 기초
- 스타터팩, 젬 패키지, 광고 보상 버튼 구조
- Unity IAP 패키지 기반 실제 결제 연결 준비
- 광고 보상 SDK 연결을 위한 보상 지급 흐름 준비
- 결제/광고 실패 및 복구 처리는 추후 강화 예정

### 설정 / 편의 기능

- 사운드 ON/OFF 설정 구조
- 진동 ON/OFF 설정 구조
- 알림 ON/OFF 설정 구조
- 30/60 FPS 전환
- 언어 전환 구조
- Jua/Jalnan2/ONE Mobile POP 계열 폰트 리소스 추가
- 아직 실제 한글 로컬라이징 완성 전이므로 일부 UI 문구는 임시 영어 중심 -> 추 후 문구 확정 및 한글화 예정

### 모바일 UI / Android 빌드

- 1080x1920 기준 세로형 모바일 UI
- Safe Area 대응 공통 레이아웃 `MobileScreenLayout`
- 긴 Android 화면 비율 대응
- 메인/뽑기 UI 공통 Canvas 구조 정리
- 주요 UI를 `Assets/Resources/Prefabs/UI` prefab 기반으로 전환
- prefab이 없거나 깨진 경우 기존 runtime 생성 fallback 사용
- `RuntimeUiBinder`로 prefab 안 버튼, 텍스트, 숫자, 진행바를 이름 기준으로 다시 연결
- `Tools > UI > Regenerate Runtime UI Prefabs` 메뉴로 런타임 UI prefab 재생성 가능
- `Tools > UI > Open UI Preview Scene` 메뉴로 UI 조정용 scene 바로 열기 가능
- `Tools > UI > Rebuild UI Preview Scene` 메뉴로 UI 조정용 preview scene 생성 가능
- `Tools > UI > Rebuild Individual UI Preview Scenes` 메뉴로 화면별 개별 preview scene 생성 가능
- `Tools > UI > Fix Battle Preview Now` 메뉴로 전투 prefab 보정, 전투 preview scene 재생성, 전투 preview scene 열기 가능
- `Tools > UI > Apply Selected Preview UI Override To Prefab` 메뉴로 preview scene에서 고친 prefab instance를 원본 prefab에 반영 가능
- 전투 prefab에는 참새이등병, 동료 3명, 고양이 적의 `ActorRoot` 위치를 Scene view Gizmo로 표시하는 guide 추가
- AndroidManifest 커스텀 설정
- POST_NOTIFICATIONS 권한 추가
- Firebase Messaging용 Android Activity 설정
- Android Debug APK 빌드 에디터 도구
- Android 빌드 준비 검증 에디터 도구
- Android 에뮬레이터 빌드/설치/실행 테스트 완료

## 프로젝트 흐름

### 1. 앱 실행

1. `MainGameScene`
2. `MainGameBootstrap`이 기본 매니저들을 생성
3. 화면 방향 Portrait
4. `MobileScreenLayout`을 통해 모바일 Safe Area UI를 구성
5. `Resources/Prefabs/UI`의 prefab UI를 우선 불러오고, 없으면 코드 생성 UI로 대체
6. 메인 UI가 먼저 생성되고 로딩 오버레이가 표시

### 1-1. 신규 유저 스토리 튜토리얼

1. 신규 유저는 첫 플레이 진입 직후 메인 스토리 튜토리얼을 본다.
2. 튜토리얼은 5~7컷 정도의 만화 형식으로 진행
3. 화면을 클릭하면 다음 컷으로 이동
4. 컷씬 종료 후 성장/전투 기능 튜토리얼로 이어지는 방향
5. 실제 아트가 들어오기 전까지는 단색 배경과 `(아트 필요)` 문구로 대체

### 2. Firebase 초기화 / 로그인

1. Firebase 의존성 및 서비스 초기화
2. 기존 로그인 사용자가 있으면 그대로 사용
3. 사용자가 없으면 게스트 계정을 자동 생성
4. Android에서는 추후 Google 버튼을 통해 게스트 계정을 Google 계정에 연결
5. Firebase, Remote Config, FCM 상태 확인

### 3. 플레이어 데이터 로드

1. Firestore에서 현재 UID 기준 플레이어 데이터를 조회
2. 기존 데이터가 없으면 신규 데이터를 생성
3. 기존 데이터 구조가 오래된 경우 `PlayerDataConverter`가 현재 구조에 맞게 보정
4. 마지막 접속 시간을 기준으로 방치 보상을 계산
5. 플레이어 데이터가 준비되면 `PlayerDataManager`에 등록

### 4. 게임 시스템 초기화

1. Monetization, PushNotification, Equipment, Companion 시스템 초기화
2. 글로벌 우편 조회
3. Growth, Battle, Tutorial, Quest, EventMission 시스템 초기화
4. 초기 데이터를 Firestore에 저장
5. 로딩 오버레이를 닫고 메인 전투 UI를 갱신

### 5. 메인 전투 루프

1. 플레이어는 메인 전투 화면에서 자동 전투
2. 몬스터 처치와 스테이지 진행이 전투 매니저에서 처리
3. 자동 진행 -> 다음 스테이지 진행
4. 동료 공격 시 투사체와 데미지 숫자를 표시
5. 성장, 장비, 동료, 퀘스트 진행 상황이 전투력과 보상 흐름에 연결

### 5-1. UI prefab 조정 흐름

1. 실제 게임은 `MainGameScene`의 `MainGameBootstrap`에서 UI를 생성
2. 조정할 UI는 `Assets/Resources/Prefabs/UI` 안의 prefab을 직접 수정
3. `Tools > UI > Open UI Preview Scene`으로 조정용 scene을 바로 열 수 있음
4. `Tools > UI > Rebuild UI Preview Scene`으로 조정용 scene을 다시 생성 가능
5. `Tools > UI > Rebuild Individual UI Preview Scenes`로 화면별 개별 preview scene 생성 가능
6. `Tools > UI > Fix Battle Preview Now`로 전투 preview scene이 낡았을 때 전투 prefab과 scene을 다시 맞춤
7. preview scene에서 prefab instance를 수정한 경우 `Overrides > Apply All` 또는 `Tools > UI > Apply Selected Preview UI Override To Prefab`으로 prefab에 반영
8. 전투 캐릭터 위치는 `EnemyActorRoot`, `SupportActorRoot`, `CompanionActorRoot1~3`를 옮겨 조정
9. `Tools > UI > Regenerate Runtime UI Prefabs`는 코드 기준 prefab 재생성용이므로 수동 수정 후에는 주의해서 사용

### 6. 성장 / 장비 / 동료

1. Growth 탭에서 골드를 사용해 능력치를 올립니다.
2. 캐릭터/동료는 도감에서 직접 선택하고 장착/해제
3. 장비는 장착 및 강화 구조를 통해 전투력을 올리는 방향으로 확장

### 7. 뽑기 흐름

1. 메인 화면의 Gacha 버튼을 누르면 전투 맵 위 뽑기 Overlay를 표시
2. 뽑기는 티켓을 먼저 사용하고 부족하면 젬을 사용
3. 뽑기 결과는 인벤토리/캐릭터 보유 데이터에 반영됩니다.
4. 뽑기 후 Firestore에 저장하고 메인 퀘스트/이벤트 미션 진행도를 갱신
5. 하단 탭으로 전투와 뽑기를 Scene 로딩 없이 전환

### 8. 퀘스트 / 보상 흐름

1. 현재 메인 퀘스트 하나만 크게 보여줌 -> 추 후 UI 변경
2. 퀘스트를 완료하면 보상을 받고 다음 퀘스트가 열림
3. 우편, 일일 보상, 이벤트 보상 LiveOps 보상 흐름
4. 추후 출석, 룰렛, 시즌패스, 기간 이벤트를 같은 구조에 연결

### 9. 저장 / 종료 흐름

1. 일정 시간마다 자동 저장
2. 뽑기 완료, 수동 저장, 앱 일시정지 시 Firestore 저장
3. 앱 종료 시 마지막 접속 시간을 갱신 -> 오프라인 보상 계산

## 현재 보류 / 추후 결정 사항

- 캐릭터 콘셉트, 이름, 관계성
- 튜토리얼 컷별 최종 대사와 픽셀 아트
- 최종 UI 색상, 폰트, 아이콘, 프레임, 배치
- 실제 영웅/동료/몬스터/보스 스프라이트
- 배경 이미지
- 스킬 이펙트
- BGM/효과음
- 보스 종류, 패턴, 보상
- 동료 조합 및 시너지 세부 규칙
- 장비 종류, 등급, 옵션
- 결제 상품 구성과 가격
- 광고 보상량
- 밸런스 수치

## 추가할 에셋

- 영웅 스프라이트
- 동료 스프라이트
- 몬스터/보스 스프라이트
- 배경 이미지
- 최종 UI 아이콘과 프레임
- 스킬 이펙트
- BGM
- 효과음
- 한글 TMP Font Asset

## 실제 아트 연결 규칙

1. `Tools > Battle Art > Prepare Production Art Folders`로 실제 아트 폴더 생성
2. 참새 이등병: `Assets/Art/Battle/Heroes/SupportSparrow/SupportSparrow.png`
3. 동료: `Assets/Art/Battle/Companions/{캐릭터이름}/{캐릭터이름}.png`
4. 일반 고양이: `Assets/Art/Battle/Enemies/{프로필명}/{프로필명}.png`
5. 보스 고양이: `Assets/Art/Battle/Bosses/{프로필명}/{프로필명}.png`
6. 프레임 애니메이션은 각 폴더 안에 `Idle`, `Attack`, `Hit`, `Death`, `Skill` 폴더를 만들고 순서대로 png 배치
7. 기본 캐릭터 프레임 수 기준은 `Idle` 6프레임, `Attack` 8프레임, `Skill` 8프레임, `Hit` 4프레임, `Death` 6프레임
8. 투사체는 캐릭터 폴더 안의 `BasicProjectile.png`, `SkillProjectile.png` 또는 `Assets/Art/Battle/Projectiles/{캐릭터이름}_BasicProjectile.png` 형식 사용
9. 파일 추가 후 `Tools > Battle Art > Auto Link Battle Visuals` 실행
10. 고양이/보스 프로필은 `BattleVisualDatabase`에서 `stageFrom`, `stageTo`, `stageCycle`, `stageCycleOffset`, `priority`로 출현 구간 설정
11. `stageTo`가 0이면 끝 스테이지 제한 없음
12. `stageCycle`이 0 또는 1이면 구간 내 모든 스테이지 출현, 2 이상이면 `stageCycleOffset`에 맞는 스테이지만 출현
13. 같은 스테이지에 여러 프로필이 맞으면 `priority`가 가장 높은 프로필끼리만 순환 선택
14. 규칙값 정리는 `Tools > Battle Art > Normalize Visual Stage Rules` 사용
15. `BattleVisualDatabase` 인스펙터에서 스테이지별 일반 고양이/보스 선택 결과와 연결된 sprite, 애니메이션, 투사체 상태를 미리 확인 가능
16. `Tools > Battle Art > Create Sample Stage Profiles`로 기본 고양이/보스 프로필과 아트 폴더를 먼저 생성 가능
17. `Tools > Battle Art > Create Character Art Folders`로 `CharacterData` 기준 동료 아트 폴더를 자동 생성 가능
18. `Tools > Battle Art > Write Art Readiness Report`로 누락된 sprite, 애니메이션, 투사체 상태를 `Logs/BattleArtReadinessReport.txt`에 출력 가능
19. 전투 배경은 `BattleStageThemeDatabase`에서 `stageFrom`, `stageTo`, `priority`로 스테이지 구간 매핑
20. 실제 배경 파일은 `Assets/Art/Battle/Backgrounds/{테마명}.png`, 선택 레이어는 `{테마명}_Midground.png`, `{테마명}_Foreground.png` 형식 사용
21. `Tools > Battle Art > Create Sample Stage Themes`로 기본 배경 테마 프로필 생성 가능

## 출시 전 제거 / 교체할 임시 요소

- 테스트용 골드/젬 100,000 지급
- 임시 캐릭터 이름과 수치
- 임시 prefab UI 아트와 runtime fallback UI 일부
- 테스트용 상점 상품
- 임시 영어 문구
- 실제 SDK 연결 전의 결제/광고 placeholder
- 밸런스 테스트용 수치
