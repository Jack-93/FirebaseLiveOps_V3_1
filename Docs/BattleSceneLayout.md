# Battle Scene Layout

## 고정 전투 좌표

전투 캐릭터 좌표는 모든 스테이지와 배경에서 동일하게 유지한다.
맵마다 발판의 재질과 형태만 바꾸고 캐릭터 중심점은 이동하지 않는다.

- 참새 이등병: 좌측 후방 지원대
- 캐릭터 1: 전투 진형의 위쪽
- 캐릭터 2: 캐릭터 1보다 오른쪽 아래
- 캐릭터 3: 캐릭터 2보다 오른쪽 아래
- 고양이 적: 가장 오른쪽 위
- 한 스테이지에 표시되는 고양이 적: 1마리

정규화 좌표:

| 대상 | X | Y |
|---|---:|---:|
| 참새 이등병 | 0.18 | 0.43 |
| 캐릭터 1 | 0.50 | 0.62 |
| 캐릭터 2 | 0.56 | 0.51 |
| 캐릭터 3 | 0.61 | 0.40 |
| 고양이 | 0.84 | 0.71 |

좌표 원본은 `BattleLayoutConfig`에서 관리한다.

## 맵 제작 규칙

- 얇은 전깃줄 위에 서 있는 연출을 강제하지 않는다.
- 나무 발판, 지붕, 간판, 배관, 성벽, 바위처럼 맵에 어울리는 바닥을 사용한다.
- 발판은 각 고정 좌표의 캐릭터 발밑을 자연스럽게 받치도록 그린다.
- 공격, 투사체, 스킬, 피격 이펙트는 고정 좌표를 기준으로 재사용한다.
- 배경과 발판이 달라져도 캐릭터 1/2/3의 순서와 상대 위치는 바꾸지 않는다.

## 스테이지 테마 에셋 규칙

`Resources/PrototypeArt` 아래에 다음 이름으로 파일을 추가하면 코드 수정 없이
10스테이지 단위로 자동 교체된다. 없는 파일은 현재 Sunset/Scout 아트로 대체된다.

| 스테이지 | 배경 이름 | 일반 적 | 보스 |
|---|---|---|---|
| 1~10 | `StageSunset.png` | `CatScout.png` | `CatScoutBoss.png` |
| 11~20 | `StageForest.png` | `CatForest.png` | `CatForestBoss.png` |
| 21~30 | `StageRooftop.png` | `CatRooftop.png` | `CatRooftopBoss.png` |
| 31 이상 | `StageRain.png` | `CatRain.png` | `CatRainBoss.png` |

선택 레이어:

- 기본 배경: `Backgrounds/StageName.png`
- 중간 장식: `Backgrounds/StageName_Midground.png`
- 전경 장식: `Backgrounds/StageName_Foreground.png`
- 적: `Enemies/CatName.png`
- 플레이어 참새 이등병: `Heroes/SupportSparrow.png`
