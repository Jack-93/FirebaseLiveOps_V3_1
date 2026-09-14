# Firebase LiveOps V3 진행 현황

- 기록일: 2026-09-15 (Asia/Seoul)
- 협업 문서: [Notion 개발 진행 현황](https://www.notion.so/3dba9c01e4068119bc02dea3ee08da05)
- 기준: 현재 로컬 코드·프리펩·데이터와 이전 작업 기록을 대조한 상태 정리
- 프로젝트: Unity 2D 세로형 전투 RPG 프로토타입
- 현재 단계: 전투·장비·성장·저장 기반 구현 후 화면 비율과 플레이 완성도를 다듬는 단계
- 정리 시작 시점의 기준 브랜치 및 커밋: `main`, `9f63431` (2026-07-20, `feat: overhaul battle and equipment progression`)
- 정리 시작 시점의 로컬 상태: 추적 파일 19개 수정 및 신규 파일·폴더 존재, 미커밋 상태
- 이전 기록상 마지막 실질 작업 시점: 2026-08-13

이 문서의 ‘구현’은 코드 또는 에셋이 존재한다는 의미입니다. 실제 플레이 검수, 외부 계정 설정, 실기기 검증 완료 여부는 별도로 표시합니다. 이번 개발 스냅샷에는 정리 시작 시점의 미커밋 게임 코드·에셋과 진행 현황 문서를 함께 포함합니다.

## 핵심 플레이 흐름

1. 참새 이등병을 터치·드래그로 이동시키며 적의 공격을 피합니다.
2. 자동 공격과 전력 충전, 동료 스킬을 사용해 일반 적을 순차 처치합니다.
3. 일반 스테이지를 돌파하고 10배수 스테이지에서 보스에 도전합니다.
4. 골드와 개별 장비를 획득하고, 장착·해체·강화·옵션 재설정으로 성장합니다.
5. 동료의 역할·속성·스킬·시너지를 조합해 비행단을 편성합니다.

## 구현된 전투 기반

- 참새 터치 이동, 자동 공격, 전력 충전 및 동료 스킬 흐름이 구현되어 있습니다.
- 적이 참새를 직접 추적하고 공격하며, 참새 체력이 0이 되면 현재 스테이지를 다시 시작합니다.
- 일반 적은 근접형 `CatMelee_1`, 마법형 `CatMage_1`, 돌진형 `CatDash_1` 3종입니다.
- 공격 타입·사거리·접근 속도·공격 간격·투사체 시간·피해 배율을 데이터로 관리합니다.
- 10스테이지 보스 캣베로스는 추적 낙뢰탄, 삼중 화염 숨결, 유령탄 연사 패턴을 사용합니다.
- 보스 패턴 예고, 안전 영역, 투사체 표현과 제한 시간 처리가 구현되어 있습니다.
- 최근 로컬 변경에는 적 추적·이동 범위 보정과 보스 경고 영역·판정 정렬이 포함됩니다.
- 난이도와 화면 가독성은 실제 플레이 기준으로 추가 조정해야 합니다.

근거: [BattleManager](../Assets/Scripts/Battle/BattleManager.cs), [BattleTouchMovementController](../Assets/Scripts/Battle/BattleTouchMovementController.cs), [EnemyCombatProfile](../Assets/Scripts/Battle/EnemyCombatProfile.cs), [BossPatternPresentation](../Assets/Scripts/UI/BossPatternPresentation.cs).

## 이번 스냅샷의 주요 변경: 일반 스테이지 웨이브

- 챕터 내 1~3번째 일반 스테이지는 적 2마리, 4~6번째는 3마리, 7~9번째는 4마리입니다.
- 여러 적이 동시에 등장하는 방식이 아니라 한 마리씩 처치하며 진행하는 순차 전투입니다.
- 10배수 스테이지는 일반 웨이브 대신 보스 1마리로 진행됩니다.
- `StageWaveDatabase`에서 적 정의와 구간별 편성 규칙을 관리합니다.
- 같은 공격 타입은 최대 2회 연속 등장하며, 챕터 내 7~9스테이지에는 세 타입을 모두 포함합니다.
- 웨이브별 체력·골드 배분과 현재 적 진행 정보를 전투·플레이어 데이터에 반영했습니다.
- 코드와 데이터는 로컬에 존재하며, 이번 정리에서는 Play Mode로 재검증하지 않았습니다.

근거: [StageWaveDatabase 코드](../Assets/Scripts/Battle/StageWaveDatabase.cs), [StageWaveDatabase 데이터](../Assets/Resources/StageWaveDatabase.asset), [GameBalance](../Assets/Scripts/Core/GameBalance.cs), [PlayerData](../Assets/Scripts/Data/PlayerData.cs).

## 장비·성장 구현

- 드랍 장비를 개별 `EquipmentInstance`로 보관하며 플레이어가 직접 장착합니다.
- 더 좋은 장비를 획득해도 기존 장비를 자동 삭제하거나 새 장비를 자동 장착하지 않습니다.
- 강화 단계와 랜덤 옵션은 슬롯이 아닌 개별 장비 인스턴스에 귀속됩니다.
- 장비 해체와 등급별 비행단 장비 코인 지급이 구현되어 있습니다.
- 최대 20성 강화, 단계별 비용·성공률, 보장 구간 및 실패 보호 규칙이 구현되어 있습니다.
- 드랍 시 0~3줄 랜덤 옵션을 부여하며, 같은 장비에서 옵션 종류가 중복되지 않도록 처리합니다.
- 옵션 재설정은 줄 수를 유지하며 기존 옵션과 신규 옵션 중 사용자가 직접 선택합니다.
- 장착·해체·강화 확인·강화 결과·옵션 비교 흐름은 프리펩 기반 모달로 연결되어 있습니다.

근거: [EquipmentManager](../Assets/Scripts/Data/EquipmentManager.cs), [EquipmentDatabase](../Assets/Resources/EquipmentDatabase.asset), [EquipmentActionController](../Assets/Scripts/UI/EquipmentActionController.cs).

## 이번 스냅샷의 주요 변경: 장비창

- 인벤토리를 5열, 최소 45칸, 세로 스크롤 구조로 개편했습니다.
- 보유 장비가 늘어나면 필요한 행 수에 맞춰 표시 칸 수가 증가합니다.
- 슬롯에 등급·강화·장비명·착용 상태를 표시합니다.
- 아이템 상세 팝업과 장착·해체 동작을 연결했습니다.
- 이전 작업 기록에서는 Unity 컴파일 오류 0개로 보고되었습니다.
- 실제 화면 배치·텍스트 크기·스크롤·터치 조작 검수는 남아 있습니다.

근거: [EquipmentInventoryModalUI](../Assets/Scripts/UI/EquipmentInventoryModalUI.cs), [EquipmentItemActionModalUI](../Assets/Scripts/UI/EquipmentItemActionModalUI.cs), [장비창 프리펩](../Assets/Resources/Prefabs/UI/EquipmentInventoryModal.prefab).

## 동료·아트 파이프라인

- `CharacterData`와 `CharacterDatabase`에 캐릭터 정의를 두고 `BattleVisualDatabase`로 전투 비주얼을 연결합니다.
- 동료 편성·역할·속성·스킬·시너지 계산을 위한 코드가 존재합니다.
- 에디터의 `Sync Battle Art Pipeline` 명령으로 아트와 캐릭터·전투 데이터 연결을 갱신할 수 있습니다.
- 아트 준비 상태 보고서와 적 Animator 생성 도구가 존재합니다.
- 일반 고양이 3종과 캣베로스, 동료 아트 및 투사체를 사용하는 전투 구조가 마련되어 있습니다.
- 맵은 개별 스프라이트를 조합한 프리펩과 `BattleStageThemeDatabase`로 구성합니다.
- 추가 적·보스·맵·정식 오디오는 콘텐츠 보강 범위에 남아 있습니다.

근거: [CompanionManager](../Assets/Scripts/Data/CompanionManager.cs), [CompanionSynergySystem](../Assets/Scripts/Data/CompanionSynergySystem.cs), [BattleArtPipelineTools](../Assets/Editor/BattleArtPipelineTools.cs), [BattleStageThemeDatabase](../Assets/Resources/BattleStageThemeDatabase.asset).

## 저장·LiveOps 기반

- `PlayerDataConverter`를 통해 플레이어 데이터를 직렬화하고 Firestore 및 로컬 캐시에 저장하는 구조가 있습니다.
- 장비 인스턴스·장착 ID·강화·옵션·비행단 장비 코인 저장과 기존 데이터 변환 처리가 구현되어 있습니다.
- 저장 예약·로컬 캐시·원격 저장 대기 상태를 관리하는 코드가 존재합니다.
- Firebase 초기화·계정 연결·Analytics·푸시 및 Remote Config를 사용하는 기반 코드가 있습니다.
- 상점·퀘스트·이벤트 미션·일일 보상·우편함 등 운영 기능의 로컬 구조가 마련되어 있습니다.
- 결제와 보상형 광고는 provider 인터페이스와 에디터용 처리 흐름이 존재합니다. 실제 상용 provider 연동 완료로 간주하지 않습니다.
- Firebase 콘솔 설정, Firestore Rules 배포, App Check, 실제 로그인·크래시·푸시 수신은 별도 확인이 필요합니다.

근거: [PlayerDataConverter](../Assets/Scripts/Data/PlayerDataConverter.cs), [PlayerDataLocalCache](../Assets/Scripts/Data/PlayerDataLocalCache.cs), [Firebase 코드](../Assets/Scripts/Firebase), [LiveOps 코드](../Assets/Scripts/LiveOps), [MonetizationManager](../Assets/Scripts/LiveOps/MonetizationManager.cs).

## 이번 작업: 전투 스타일 슬라이스 1차

- 기존 전투 로직과 `BattleHud` 프리펩을 유지하면서 밝고 둥근 캐주얼 전투 발표 레이어를 추가했습니다.
- 상단에 `AUTO FLOW`, `보스 · 위기 때 직접 조작`, 현재 스테이지 또는 `BOSS WAVE`를 표시합니다.
- 자동 공격·동료 공격·전력 충전·적 접근·공격 예고·보스 패턴·피격·회복·웨이브 클리어를 카드와 콜아웃으로 연결했습니다.
- 전장에 따뜻한 색 틴트를 적용하고 위기·보스 상태에서 위험 색과 펄스를 사용합니다.
- 평소 자동 전투, 보스·위기 순간 직접 이동이라는 조작 방향을 화면에서 확인할 수 있습니다.
- 새 원화 없이 기존 로컬 아트와 런타임 UI를 사용했습니다. 이미지 생성 도구는 사용량 제한으로 실행할 수 없어 후속 아트 교체 지점만 열어 두었습니다.
- 실제 카드 위치·한글 폰트·기기 화면 비율은 Unity `Preview_00_Battle` Play Mode 검수가 남아 있습니다.

근거: [전투 스타일 슬라이스](BattleStyleSlice-2026-09-15.md), [BattleNyangStylePresentation](../Assets/Scripts/UI/BattleNyangStylePresentation.cs), [BattleHudUI](../Assets/Scripts/UI/BattleHudUI.cs).

## 검수 대기·미구현 항목

- 공통 UI 프리펩 12종은 생성되어 있으나 검수 대기 상태이며 기존 화면에는 아직 적용하지 않았습니다.
- 구성은 버튼 5종, 대화상자 2종, 장비·비용·옵션·별 표시 요소 5종입니다.
- UI와 주인공·동료·일반 적·보스가 전반적으로 크게 보인다는 피드백이 있습니다.
- 공통 배율을 한 곳에서 관리하는 방안은 제안 단계이며 아직 구현하지 않았습니다.
- 현재 크기 조절의 다음 목표는 화면 점유율·전투 가독성·터치 조작성의 균형을 확인하는 것입니다.

근거: [공통 UI 프리펩](../Assets/Resources/Prefabs/UI/Common), [BattleHudUI](../Assets/Scripts/UI/BattleHudUI.cs), [MobileScreenLayout](../Assets/Scripts/UI/MobileScreenLayout.cs).

## 검증 범위와 문서 정합성

- 이전 검증 기록: 장비창 개편 당시 Unity 컴파일 오류 0개로 보고되었습니다. 당시의 결과이며 현재 전체 상태에 대한 새 검증 결과는 아닙니다.
- 이번 확인: 로컬 코드·에셋·Git 상태와 기존 기록을 대조하고 변경 파일의 정적 구조를 검사했습니다.
- 변경·신규 `Assets` 파일 51개에서 충돌 표식, meta 누락·고아 meta·상위 폴더 meta 누락은 발견되지 않았습니다.
- 프리펩 16개와 씬 1개에서 중복 fileID 및 GUID 없는 단순 로컬 참조 누락은 발견되지 않았습니다. 외부 GUID 참조 유효성은 검증하지 않았습니다.
- C# 변경의 `git diff --check`는 통과했습니다. `BattleHud.prefab`에는 빈 YAML 필드 뒤 후행 공백 경고 17개가 있으며, 그 자체가 기능 오류의 증거는 아닙니다.
- 이번 미실행: Unity 컴파일, Play Mode, Android 실기기, 릴리스 빌드, Firebase 운영 환경 테스트.
- 기존 문서와 코드가 다르면 현재 코드를 우선했습니다.
- `PrototypeMinimumGold`와 `PrototypeMinimumGems`는 이미 `0`입니다. 이를 0으로 변경하는 작업은 남은 작업에서 제외합니다.
- 정리 시작 시점의 README는 일반 적 순차 웨이브를 설명하지 않았고, 외부 작업 목록은 이미 0인 프로토타입 재화를 변경할 항목으로 남겨 두고 있었습니다. 이번 정리에서는 현재 코드 기준으로 바로잡습니다.

근거: [GameBalanceConfig](../Assets/Scripts/Core/GameBalanceConfig.cs), [README](../Readme.md), [외부 작업 목록](RemainingExternalTasks.md).

## 다음 작업 체크리스트

- [ ] Unity에서 장비창 5열·45칸·세로 스크롤과 상세 팝업을 검수합니다.
- [ ] UI·주인공·동료·일반 적·보스의 공통 크기 설정을 구현하고 실제 화면 비율을 조정합니다.
- [ ] 일반 구간 2→3→4마리 순차 전투, 적 교체, 체력·골드 배분, 패배 후 재시작을 확인합니다.
- [ ] 캣베로스 경고 영역·안전 영역·피격 판정·제한 시간과 전투 난이도를 확인합니다.
- [ ] 장비 드랍·장착·해체·강화·옵션 선택 후 저장 및 재접속 복원을 확인합니다.
- [ ] 공통 UI 12종을 검수하고 적용 범위와 교체 순서를 확정합니다.
- [ ] 추가 일반 적·20/30스테이지 보스·맵 변형·정식 UI·오디오를 보강합니다.
- [ ] Android 실기기에서 로그인·화면 비율·성능·저장·푸시·크래시 수집을 확인합니다.
- [ ] Firebase 운영 설정과 보안 규칙을 확인하고 실제 결제·광고 provider 및 보상 검증을 완료합니다.
- [ ] 서명·Release AAB·내부 테스트·스토어 자료·개인정보 및 데이터 삭제 안내 등 출시 준비를 진행합니다.

## 저장소 연결

- [GitHub 진행 현황 문서](https://github.com/Jack-93/FirebaseLiveOps_V3_1/blob/main/Docs/ProgressStatus-2026-09-15.md)
- 전투 스타일 슬라이스 1차를 `main`에 저장했습니다.
- 저장 커밋: [e99160c — feat: add nyang battle style slice](https://github.com/Jack-93/FirebaseLiveOps_V3_1/commit/e99160c)
- 현재 로컬 작업 트리는 깨끗하며 로컬·원격 앞섬/뒤처짐은 0입니다.
