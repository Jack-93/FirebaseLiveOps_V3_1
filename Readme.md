# Firebase LiveOps v3.1

- Unity 6000.4.8f1
- Firebase SDK 13.11.0
- Android Bundle ID: com.DoOurGame.gameliveops (Firebase 설정과 Bundle ID는 현재 일치)
- 주요 기술: Authentication, Firestore, Analytics, Remote Config, LiveOps

## 필요 Firebase 패키지
- FirebaseApp
- FirebaseAnalytics
- FirebaseRemoteConfig

## Firebase SDK 다운로드
https://firebase.google.com/download/unity

## 포함 기능
- Firebase 초기화 및 연결 확인
- 익명 로그인, 자동 로그인, 로그아웃, UID 표시
- PlayerData 기본 구조
- 닉네임, 골드, 인벤토리 추가·삭제
- Firestore 기본 저장
- 가챠 단차와 10연차
- 100회 천장과 10연차 SR 이상 보장
- Remote Config 기반 SSR/SR 확률
- ScriptableObject 캐릭터 데이터베이스
- 등급별 가챠 결과 슬롯과 ScrollView
- Firebase Analytics 로그인·가챠·SSR 이벤트
- 7일 출석 보상 로직
- 우편 생성, 개별 수령, 전체 수령
- Firestore global_mails 운영 우편 조회
- Lobby와 Gacha 씬 이동
- 우편·일일보상 UI 보완
- 동료 개별 선택·장착·해제 화면-
- 캐릭터 도감과 상세정보 UI-
- 스킬 및 쿨타임 시스템-
- 스테이지별 적 구성과 보스전-
- 스테이지 선택·자동 진행 설정-
- 장비 획득·장착·강화 시스템-
- 퀘스트·업적 시스템


## 프로젝트 흐름
### 유저 데이터 생성 저장
Firebase Auth
Firestore
Inventory
Analytics
Remote Config
Gacha
Daily Reward

### Live Ops
Mail Box (Reward)

## 계획 중
튜토리얼 전체 흐름 확장
상점·패키지·광고 보상
뽑기 연출·배너·확률표·천장 개선
이벤트·출석·기간 콘텐츠
설정·사운드·알림·언어 UI
실제 세계관·캐릭터·스토리 적용
UI 디자인 및 모바일 해상도 최적화
Firebase 보안 규칙·서버 검증 강화
저장 충돌·오프라인·네트워크 복구 처리
Android 실기기 테스트와 성능 최적화
밸런스 조정·분석 이벤트 정리
빌드·배포·스토어 출시 준비

## 남은 주요 작업
실제 캐릭터·몬스터 스프라이트 적용
공격·피격·사망·스킬 애니메이션
보스별 패턴과 추가 전투 콘텐츠
동료 조합·속성·시너지 시스템
스토리와 튜토리얼 전체 확장
로그인 계정 연동: Google·Apple 등
실제 결제 패키지와 광고 보상
기간 이벤트·출석 콘텐츠 확장
가챠 연출·배너·상세 확률 UI
실제 BGM·효과음·진동 연결
실제 모바일 푸시 알림 연동
Android 실기기 테스트와 최적화
Firebase 보안 규칙·서버 검증
오프라인 저장·충돌 복구 강화
밸런스 조정과 출시 준비

## 구상 및 보류
세계관
메인 스토리
캐릭터 외형·이름·설정
캐릭터 관계와 개별 스토리
UI 디자인·색상·버튼 위치
성장·전투·퀘스트·상점 보상 수치
캐릭터 등급 및 가챠 확률
동료 조합과 시너지 규칙
보스 종류·패턴·보상
장비 종류·등급·옵션
이벤트 주제와 운영 방식

## 추가할 에셋
캐릭터·몬스터·보스 스프라이트
배경·아이콘·UI 이미지
스킬 이펙트
BGM·효과음
Noto Sans KR 같은 한글 폰트와 TMP Font Asset

## 출시 전 제거·교체할 임시 요소
골드·젬 최소 10만 지급
임시 캐릭터 이름과 수치
코드로 생성되는 프로토타입 UI
테스트용 상점 상품
시스템 폰트를 사용하는 임시 한글 처리
알림 설정만 있고 실제 알림 SDK가 없는 상태
