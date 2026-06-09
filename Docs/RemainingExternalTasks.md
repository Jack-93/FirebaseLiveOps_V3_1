# Remaining External Tasks

현재 로컬 코드와 자동 검증으로 처리할 수 있는 작업은 완료했다.
아래 항목은 외부 에셋, 실제 계정 설정, 실기기 또는 출시 결정이 필요하다.

## 아트 / 사운드

- 타이틀 배경과 게임 로고
- 전투 배경
- 영웅, 동료, 몬스터, 보스 스프라이트
- 장비와 재화 아이콘
- 스킬 및 뽑기 이펙트
- BGM과 효과음

## 기획 결정

- 세계관과 메인 스토리
- 튜토리얼 전체 흐름
- 최종 캐릭터 이름과 설정
- 동료 시너지 세부 규칙
- 보스 패턴과 보상
- 실제 경제 밸런스와 결제 상품 가격

## Google / Firebase / Play Console

- Play Console 개발자 계정 실기기 확인
- Play App Signing 활성화
- App Signing SHA-1/SHA-256을 Firebase에 등록
- 갱신된 `google-services.json` 최종 확인
- 실제 Android 기기에서 Google 로그인 확인
- Firestore Rules 배포
- Firebase App Check Play Integrity 활성화
- Crashlytics 크래시 수신 확인
- Firebase Messaging 실기기 푸시 확인

## 결제 / 광고

- Google Play Billing 상품 생성
- Unity IAP 실제 provider 연결
- 광고 SDK 선택과 provider 연결
- 구매 영수증 서버 검증
- 광고 보상 실기기 검증

## 출시 전

- `GameBalanceConfig.PrototypeMinimumGold`를 `0`으로 변경
- `GameBalanceConfig.PrototypeMinimumGems`를 `0`으로 변경
- 릴리즈 키스토어 생성 및 안전한 별도 보관
- Release AAB 빌드
- 내부 테스트 트랙 업로드
- 개인정보처리방침과 데이터 삭제 안내
- 스토어 설명, 스크린샷, 아이콘, 피처 그래픽
- 콘텐츠 등급, 데이터 보안, 광고 여부 설문
