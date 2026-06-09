# Idle RPG Reference Study

이 문서는 2D 세로형 방치 RPG 제작 판단을 위한 내부 참고 노트다.
아트 에셋을 넣기 전까지는 UI 구조, 정보 우선순위, 한글 문구 톤,
성장 루프, 보상 설계 방향을 결정할 때 참고한다.

## 참고 타이틀

- Cat Hero: Idle RPG
- Legend of Slime
- Blade Idle
- MapleStory: Idle RPG
- 기타 세로형 idle / gacha / companion RPG

## 공통 구조

세로형 방치 RPG는 대부분 아래 흐름을 유지한다.

- 상단: 스테이지, 주요 재화, 전투력
- 중앙: 자동 전투 연출과 현재 적/보스 상태
- 하단: Battle, Growth, Gacha, More 같은 고정 내비게이션
- 보조 화면: 성장, 장비, 동료, 퀘스트, 이벤트, 상점
- 재방문 동기: 방치 보상, 일일 보상, 이벤트 미션, 무료 광고 보상

우리 게임도 이 구조를 이미 따르고 있으므로, 당장은 큰 화면 구조를
바꾸기보다 가독성과 귀여운 톤을 강화하는 쪽이 좋다.

## UI 방향성

좋은 방향:

- 한 화면의 핵심 행동을 1~2개로 제한한다.
- 성장/장착/뽑기/보상 버튼은 늘 “지금 누르면 무엇을 얻는지”를 보인다.
- 숫자는 크게, 설명은 짧게, 보조 정보는 카드 안에 작게 둔다.
- 빨간 알림점과 메뉴 수를 과하게 늘리지 않는다.
- 한글은 영어보다 길어지므로 버튼 라벨은 2줄까지 자연스럽게 허용한다.
- 귀여운 게임일수록 UI도 과한 금속/네온보다 둥글고 밝은 대비가 어울린다.

피해야 할 방향:

- 시작부터 메뉴를 너무 많이 열어둔다.
- 재화 종류를 초반부터 과하게 늘린다.
- “강화 가능”, “보상 가능”을 전부 빨간 점으로만 해결한다.
- 확률/광고/결제 안내를 흐리게 숨긴다.
- 버튼 문구가 영어와 한글이 섞여 보이게 둔다.

## 한글화 방향

한글 UI는 직역보다 짧고 명확한 기능어가 좋다.

- BATTLE -> 전투
- GROWTH -> 성장
- GACHA -> 뽑기
- MORE -> 더보기
- EQUIP -> 장착
- REMOVE -> 해제
- READY -> 준비됨
- CLAIM -> 받기
- POWER -> 전투력
- COST -> 비용

긴 문장보다는 플레이어 행동 중심으로 쓴다.

- “계속 플레이해서 목표를 완료하세요.”
- “동료를 선택한 뒤 파티 슬롯에 장착하세요.”
- “골드를 사용해 영웅을 강화하세요.”
- “뽑기에서 동료를 획득하세요.”

나중에 번역량이 늘어나면 `LocalizationManager`의 코드 테이블을
CSV 또는 ScriptableObject 기반으로 옮긴다.

## 성장 루프 방향

초반 루프는 단순해야 한다.

1. 자동 전투로 골드를 얻는다.
2. 골드로 영웅을 강화한다.
3. 스테이지를 민다.
4. 뽑기로 동료를 얻는다.
5. 동료를 장착해 전투력이 오른다.
6. 퀘스트/이벤트/방치 보상이 다시 성장을 밀어준다.

중반 이후에 추가하기 좋은 축:

- 동료 시너지
- 장비 등급과 승급
- 보스 패턴
- 던전별 재화
- 스킨/꾸미기
- 시즌 이벤트

## 동료 / 장비 방향

Cat Hero 계열처럼 동료는 “여러 명 장착 + 조합 보너스”가 어울린다.
단, 초반 UI에서는 모든 시너지를 한 번에 보여주기보다 “현재 팀 공격력”
정도만 먼저 보여주는 편이 좋다.

장비는 처음에는 자동 장착과 강화 중심으로 둔다. 장비 비교, 세트 옵션,
잠재 옵션은 나중에 콘텐츠가 충분해진 뒤 추가한다.

## 뽑기 / 과금 방향

최근 플레이어 반응은 과한 가챠, 강제 광고, 복잡한 패키지에 민감하다.
우리 게임은 귀여운 톤과 오래 가는 신뢰를 목표로 하므로 아래 방향이 좋다.

- 확률과 천장을 명확히 표시한다.
- 초반 핵심 진행은 결제 없이 가능해야 한다.
- 광고 보상은 선택형으로 둔다.
- 일일 제한과 쿨타임은 UI에 솔직하게 표시한다.
- 스타터팩은 있더라도 “없으면 못 하는 느낌”을 주지 않는다.

## 아트가 들어오면 우선 적용할 곳

1. 타이틀 배경과 로고
2. 전투 배경
3. 영웅 기본 스프라이트
4. 초반 몬스터 3~5종
5. 동료 아이콘과 전투용 스프라이트
6. 뽑기 배너
7. 장비 아이콘
8. 보상/재화 아이콘

## 우리 프로젝트에 적용할 기준

- 현재 구조는 유지한다.
- UI는 카드형, 큰 버튼, 짧은 문구를 기본으로 한다.
- 한글 우선 가독성을 기준으로 폰트 크기와 자동 축소를 조정한다.
- 밸런스 값은 `GameBalanceConfig`에서 먼저 조정한다.
- 에셋 추가 전에는 `(아트 필요)` 영역을 명확한 placeholder로 둔다.
- 출시 전에는 `PrototypeMinimumGold`와 `PrototypeMinimumGems`를 0으로 돌린다.

## 참고 링크

- Cat Hero official: https://www.gameduo.net/en/game/gv
- Cat Hero Google Play: https://play.google.com/store/apps/details?id=net.gameduo.gv
- Cat Hero guide: https://www.talkandroid.com/29615-cat-hero-idle-rpg-ultimate-gameplay-guide/
- Blade Idle guide: https://www.pocketgamer.com/blade-idle/guide/
- Legend of Slime discussion/review context: https://www.mobilegamereport.com/articles/legend-of-slime-underrated-2026
- Mobile monetization report context: https://files.gameindustrylibrary.com/documents/mobile-monetization-report-2025.pdf
