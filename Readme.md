# Firebase LiveOps V3.1

Unity 2D 세로형 전투 RPG 프로토타입. 고양이 습격으로 터전을 잃은 참새 이등병이 까마귀 연구소의 지원을 받아 비행단을 꾸리고 터전을 되찾는 게임이다.

## 개발 환경

- Unity `6000.4.8f1`
- Firebase Unity SDK `13.11.0`
- Firebase Authentication, Firestore, Analytics, Remote Config, Crashlytics, Messaging
- Unity IAP 및 보상형 광고 연동 지점
- Android 패키지: `com.DoOurGame.gameliveops`

## 현재 게임 흐름

1. 참새 이등병을 터치로 이동시키며 고양이의 공격을 피한다.
2. 전력 충전으로 스킬 자원을 모으고 동료와 함께 적을 자동 공격한다.
3. 일반 스테이지를 돌파하고 10배수 스테이지의 보스를 처치한다.
4. 골드, 장비, 비행단 장비 코인을 획득해 캐릭터와 장비를 성장시킨다.
5. 동료의 역할, 속성, 스킬, 시너지를 조합해 비행단을 편성한다.

## 전투 시스템

- 전봇대 내구도 시스템은 제거됐다. 고양이는 참새 이등병을 직접 추적하고 공격한다.
- 참새 이등병의 체력이 0이 되면 현재 스테이지를 처음부터 다시 시작한다.
- 명중률과 회피율은 없다. 일반 공격과 스킬 타격은 자동 처리된다.
- 플레이어는 전투 영역을 터치하거나 드래그해 참새 이등병을 이동시킨다.
- 전력 충전은 기본적으로 한 번에 `5`씩 증가한다.
- 렌더링과 Animator 클립은 `60 Samples` 기준을 유지한다.
- `BattleTempo.SimulationSpeed`는 `0.5`다. 전투 진행과 이동은 기존 기준의 절반 속도로 동작한다.
- 스테이지 `1~9`는 근접, 원거리 마법, 돌진 타입 중 하나를 무작위 선택한다.
- `10`배수 스테이지는 보스 스테이지다.
- 고양이별 공격 타입, 사거리, 접근 속도, 공격 간격, 투사체 시간, 피해 배율은 `EnemyCombatProfile`에서 관리한다.

### 일반 고양이

- `CatMelee_1`: 참새에게 접근해 근접 공격한다.
- `CatMage_1`: 지정 사거리까지 접근한 뒤 `CatMage_1_ArcaneBolt`를 발사한다.
- `CatDash_1`: 빠르게 접근해 돌진 공격한다.
- 각 일반 고양이는 Idle, Move, Attack, Death 8프레임 애니메이션과 `60 Samples` Animator를 사용한다.
- 같은 전투 타입의 추가 아트는 `CatMelee_2`, `CatMage_2`, `CatDash_2`처럼 번호를 증가시킨다.
- 기존 프로토타입 고양이 아트는 삭제하지 않고 향후 콘텐츠용으로 보존한다.

### 10스테이지 보스: 캣베로스

- 머리 세 개가 달린 고양이 보스다.
- 제자리에서 일반 공격 없이 예고형 스킬만 사용한다.
- 패턴 순서: `추적 낙뢰탄 -> 삼중 화염 숨결 -> 유령탄 연사`.
- 추적 낙뢰탄은 현재 참새 위치를 중심으로 공격 범위를 예고한다.
- 삼중 화염 숨결은 3개 레인 중 안전 레인 하나를 남긴다.
- 유령탄 연사는 3개 목표 지점을 예고한 뒤 연속 공격한다.
- 패턴 예고와 투사체는 `BattleHud.prefab`의 프리펩 오브젝트를 사용한다.
- 기본 제한 시간은 시뮬레이션 시간 기준 `40초`다.
- 보스 패턴 수치는 `BossPatternDatabase`에서 관리한다.

## 동료 및 아트 제작 규칙

- 캐릭터 데이터는 `CharacterData`, 전투 비주얼은 `BattleVisualDatabase`에서 연결한다.
- 신규 동료는 Hit 모션을 만들지 않는다. 피격은 별도 타격 이펙트로 표현한다.
- Idle: 8프레임, 2 x 4 배열.
- Attack: 8프레임, 2 x 4 배열.
- Skill: 8프레임, 2 x 4 배열.
- Death: 8프레임, 2 x 4 배열.
- 기본 프레임 크기는 `400 x 320`이다. 공격과 스킬 이펙트가 크면 프레임을 더 크게 사용한다.
- 시트 안 프레임은 이펙트가 옆 프레임으로 넘어가지 않도록 충분한 간격을 둔다.
- 동료 아트: `Assets/Art/Battle/Companions/{CharacterName}`
- 적 원본 아트: `Assets/Art/Battle/Enemies/{EnemyName}`
- 적 런타임 애니메이션: `Assets/Resources/Battle/Enemies`
- 일반 공격 및 스킬 투사체는 `CharacterData`에서 개별 지정한다.

## 장비 시스템

장비 정의는 `Assets/Resources/EquipmentDatabase.asset`에서 관리한다.

- 무기 키: `equip101`부터 `equip104`.
- 방어구 키: `equip201`부터 `equip204`.
- 장비 이름은 Inspector의 한글 표시명을 사용한다.
- 드랍 장비는 개별 `EquipmentInstance`로 저장된다.
- 상위 장비를 얻어도 자동 장착하거나 기존 장비를 삭제하지 않는다.
- 플레이어가 `옷입히기` 또는 `비행단 물자`에서 직접 장비를 선택하고 장착한다.
- 강화 수치와 랜덤 옵션은 장비 슬롯이 아니라 개별 장비 인스턴스에 귀속된다.
- 장착 장비의 아트, 이름, 옵션, 강화 별 개수를 UI에 표시한다.
- 장비 해체 시 등급에 따라 비행단 장비 코인 `5 / 15 / 40 / 100`개를 지급한다.

### 랜덤 옵션

- 장비 드랍 시 옵션 줄 수를 `0~3줄`에서 무작위 결정한다.
- 옵션 값은 줄마다 `1~15%`다.
- 같은 장비 안에서 동일한 옵션 종류는 중복되지 않는다.
- 무기 옵션: 공격력, 스킬 피해, 보스 피해.
- 방어구 옵션: 참새 체력, 피해 감소, 회복량, 회복 속도.
- 0줄 장비는 옵션 재설정을 할 수 없다.
- 옵션 재설정은 기존 줄 수를 유지하고 옵션 종류와 값만 다시 결정한다.

### 부리부리 강화

- 최대 강화 단계는 `20성`이다.
- 성공률과 비용은 강화 단계에 따라 변한다.
- `5`, `10`, `15성` 도달 시 보장 규칙을 적용한다.
- 10성 미만 실패는 현재 단계를 유지한다.
- 10성 이상 실패는 한 단계 하락한다.
- 연속 하락 실패 보호와 다음 강화 성공 보장 규칙이 있다.
- 장비는 파괴되지 않는다.
- 강화 확인, 성공·실패 결과, 같은 장비 반복 강화 UI를 제공한다.

### 옵션 재설정

- 비행단 장비 코인만 사용한다.
- 비용은 등급별 `5 / 10 / 30 / 75`개다.
- 재설정 후 기존 옵션과 새 옵션을 비교하는 창을 표시한다.
- 플레이어가 `기존 옵션 유지` 또는 `새 옵션 적용`을 직접 선택한다.
- 선택 전까지 기존 옵션은 변경되지 않는다.
- 같은 장비를 계속 재설정할 수 있다.

## 장비 및 내비게이션 UI

- 장비 탭 이름은 `옷입히기`다.
- 전체 장비 인벤토리 탭 이름은 `비행단 물자`다.
- 무기 또는 방어구 칸을 누르면 보유 장비 전체를 아트와 이름이 있는 슬롯 형태로 표시한다.
- 장비 선택, 장착, 해체, 부리부리 강화, 옵션 재설정 흐름은 프리펩 기반 모달을 사용한다.
- 주요 장비 프리펩은 `Assets/Resources/Prefabs/UI`에 있다.
- `BottomNavigation`의 버튼 크기와 위치는 프리펩이 소유한다. 런타임 코드에서 강제로 재배치하지 않는다.
- `MorePanel`에는 별도의 장비 버튼을 두지 않는다.

## 맵 제작 방식

- 완성 배경 PNG 한 장을 세우지 않고 개별 스프라이트 에셋을 조립한다.
- 맵 에셋: `Assets/Art/Battle/MapAssets`
- 배경 에셋: `Assets/Art/Battle/Backgrounds`
- 현재 주요 세트: `RooftopCityHQ`, `AlleyResidential`, `CityPark`.
- 하늘, 원경 건물, 중경 오브젝트, 지면 타일, 발판, 전경 효과 순으로 레이어를 구성한다.
- 바닥과 발판 타일은 이어 붙일 수 있는 정면 2D 또는 수직 탑뷰 형태로 제작한다.
- 스테이지 맵 프리펩은 `Assets/Resources/Prefabs/Maps`에 둔다.
- 스테이지 구간별 `mapPrefabPath`는 `BattleStageThemeDatabase`에서 지정한다.
- 사용하지 않는 `WorldBackdrop.prefab`과 임시 맵·전투 Builder는 제거됐다.

## 저장 데이터

`PlayerData`는 `PlayerDataConverter`를 통해 Firestore와 로컬 저장 데이터로 직렬화된다.

- 장비 인스턴스, 장착 인스턴스 ID, 강화 단계, 랜덤 옵션을 저장한다.
- 비행단 장비 코인을 저장한다.
- 기존 장비 데이터는 현재 인스턴스 기반 구조로 자동 변환한다.
- 기존 슬롯 귀속 강화 수치는 장착 장비 인스턴스로 이전한다.

## 주요 경로

- 전투 밸런스: `Assets/Scripts/Core/GameBalanceConfig.cs`
- 전투 진행: `Assets/Scripts/Battle/BattleManager.cs`
- 전투 속도: `Assets/Scripts/Battle/BattleTempo.cs`
- 적 전투 타입: `Assets/Scripts/Battle/EnemyCombatProfile.cs`
- 터치 이동: `Assets/Scripts/Battle/BattleTouchMovementController.cs`
- 보스 패턴: `Assets/Scripts/Battle/BossPatternDatabase.cs`
- 장비 규칙: `Assets/Scripts/Data/EquipmentManager.cs`
- 장비 정의: `Assets/Resources/EquipmentDatabase.asset`
- 캐릭터 정의: `Assets/Resources/CharacterDatabase.asset`
- 스테이지 테마: `Assets/Resources/BattleStageThemeDatabase.asset`
- 메인 UI: `Assets/Scripts/UI/MainGameUI.cs`
- 장비 UI: `Assets/Scripts/UI/EquipmentPanelUI.cs`
- 적 Animator 생성기: `Assets/Editor/BattleEnemyAnimatorAssetGenerator.cs`

## 확인 절차

- Unity `MainGameScene` Play Mode에서 전투와 저장 데이터 변환을 확인한다.
- 1~9스테이지에서 세 가지 일반 고양이의 추적, Move, 공격, Death 모션을 확인한다.
- 10스테이지에서 캣베로스의 크기, 패턴 예고 위치, 안전 영역, 투사체, 제한 시간을 확인한다.
- 참새 이등병 터치 이동, 체력 0 재시작, 전력 충전, 스킬 발동을 확인한다.
- 장비 드랍, 수동 장착, 해체, 부리부리 강화, 옵션 재설정, 저장 후 복원을 확인한다.
- 스테이지 구간별 StageMap 프리펩 교체와 UI 프리펩 배치를 확인한다.

## 다음 작업

- Play Mode 결과를 기준으로 일반 스테이지와 캣베로스 난이도를 조정한다.
- `CatMelee_2`, `CatMage_2`, `CatDash_2` 등 같은 타입의 추가 고양이 아트를 제작한다.
- 20, 30스테이지 보스와 신규 예고형 패턴을 추가한다.
- 스테이지 맵 프리펩 변형과 콘텐츠용 프로토타입 고양이 활용처를 확정한다.
- 남은 임시 UI와 오디오를 정식 에셋으로 교체한다.
- Android 기기와 Firebase 릴리스 설정을 최종 검증한다.
