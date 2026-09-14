# 전투 스타일 슬라이스 1차

## 목적

냥찍 히어로즈에서 참고한 밝고 둥근 캐주얼 전투 인상을 현재 전투 로직에 얹어 화면 단조로움을 줄인다. 자동 전투를 기본으로 두고 보스·위기 순간에 직접 개입해야 한다는 전투 방향을 화면에서 바로 읽히게 만든다.

## 적용 범위

- 기존 `BattleHud` 프리팹을 유지한다.
- 런타임에 `BattleNyangStylePresentation`을 붙인다.
- 기존 공격, 보스 패턴, 웨이브, 이동 입력 이벤트를 시각 피드백으로 연결한다.
- 새 원화가 준비되면 `BattleVisualDatabase` 또는 `PrototypeBattleArt`의 스프라이트만 교체할 수 있다.

## 화면 변화

- 전투 상단에 `AUTO FLOW`와 `보스 · 위기 때 직접 조작` 안내를 표시한다.
- 현재 스테이지 또는 `BOSS WAVE`를 같은 카드 안에 표시한다.
- 자동 공격과 동료 공격을 `AUTO STRIKE`, `PARTY LINK` 콜아웃으로 표시한다.
- 공격력 충전 시 `POWER 00/100` 피드백을 표시한다.
- 적의 접근·공격 예고·보스 패턴 경고·시전·피격·회피를 중앙 위기 카드로 표시한다.
- 웨이브 클리어와 보스 격파를 별도 콜아웃으로 표시한다.
- 전장에 낮은 강도의 따뜻한 색 틴트를 적용하고 위기 중에는 위험 색으로 전환한다.

## 조작 규칙

1. 평소에는 동료와 영웅이 자동으로 공격한다.
2. `위기 · 접근`, `위기 · 공격 예고`, `BOSS · 패턴명`이 뜨면 `GamePlayLine`을 드래그해 영웅을 이동한다.
3. 보스 시전 중에는 패턴 표시와 위기 카드를 함께 보고 안전 위치를 선택한다.
4. 기존 스킬 버튼과 충전 규칙은 그대로 사용한다.

## 교체 지점

- HUD 발표 레이어: `Assets/Scripts/UI/BattleNyangStylePresentation.cs`
- 이벤트 연결: `Assets/Scripts/UI/BattleHudUI.cs`
- 영웅·적·투사체 시각 교체: `Assets/Scripts/Battle/PrototypeBattleArt.cs`, `Assets/Resources/BattleVisualDatabase.asset`

## 검증 상태

- `git diff --check` 통과.
- Unity 6000.4.8f1 배치 실행은 라이선스 미활성으로 종료 코드 198을 반환했다.
- 기존 생성 C# 프로젝트의 Firebase 참조 누락 때문에 전체 `dotnet build`는 실패했다. 변경 파일에서 새 스타일 클래스 관련 오류는 확인되지 않았다.
- 에디터 또는 기기에서 `Preview_00_Battle`을 실행해 카드 위치와 한글 폰트 크기를 최종 확인해야 한다.
