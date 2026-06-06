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

// 중복 캐릭터 성장·승급 시스템-
//  전투 캐릭터·적 스프라이트 적용-
//  공격·피격·사망 애니메이션-
