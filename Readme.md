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


## 프로젝트 흐름
### 유저 데이터 생성 저장
Firebase Auth
↓
Firestore
↓
Inventory
↓
Analytics
↓
Remote Config
↓
Gacha
↓
Daily Reward

### Live Ops
Mail Box (Reward)