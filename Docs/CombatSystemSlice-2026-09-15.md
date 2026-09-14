# 전투 시스템 개편 1차

기준일: 2026-09-15

## 목표

평소 자동 전투를 유지하면서 적의 공격 예고가 실제 이동 판단으로 이어지게 한다. 보스·위기 때 직접 조작한다는 방향을 일반 전투에도 확장한다. 캐릭터 원화와 전투 규칙은 분리한다.

## 변경 전

- 일반 적 공격은 공격 예고 후 항상 피해를 준다.
- `heroBattlePosition`은 보스 패턴의 안전 영역 판정에만 사용한다.
- 근접 적이 플레이어를 추적해도 공격 중 이동으로 얻는 보상이 없다.

## 변경 후

- 일반 적 공격 시작 시 현재 플레이어 위치를 공격 목표로 저장한다.
- 근접·돌진 공격은 0.58초 준비 시간을 가진다.
- 근접 적은 공격 시작 시 저장한 위치를 향해 접근한다. 준비 중 플레이어가 이동하면 적이 이동을 따라가지 않는다.
- 공격 판정 시 저장 위치에서 일정 거리 이상 벗어나면 피해를 0으로 처리한다.
- 회피 성공은 기존 `OnEnemyAttackPerformed(0)` 흐름으로 전달하고, HUD에 `회피 성공` 콜아웃·민트 스파클을 표시한다.
- 적중 시 기존 피해·피격 효과를 유지한다.
- 원거리 투사체도 발사 시 저장한 목표와 현재 위치를 비교해 회피할 수 있다.

## 유지 범위

- `BattleVisualDatabase`, `BattleActorVisualSet`, `BattleNyangCharacterStyle` 연결은 유지한다.
- 동료 자동 공격, 전력 충전, 동료 스킬, 보스 3패턴, 일반 웨이브 순차 진행은 유지한다.
- 다중 적 동시 전투와 보스 HP 페이즈는 다음 전투 개편 범위로 남긴다.

## 구현 파일

- [BattleManager](../Assets/Scripts/Battle/BattleManager.cs): 공격 목표 잠금·준비 시간·회피 판정.
- [BattleMeleeMovementController](../Assets/Scripts/Battle/BattleMeleeMovementController.cs): 공격 중 목표 위치 잠금.
- [BattleHudUI](../Assets/Scripts/UI/BattleHudUI.cs): 위기 문구와 회피 피드백.

## 검증

- `CombatManagerCompile.csproj`: 경고 0개·오류 0개.
- `IntegrationCompile.csproj`: 경고 0개·오류 0개.
- `git diff --check`: 통과.
- Unity Play Mode와 Android 실기기 검증은 Unity 라이선스 제한으로 남아 있다.
