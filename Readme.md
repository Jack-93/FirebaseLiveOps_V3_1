# Firebase LiveOps v3.1

Unity + Firebase Analytics + Remote Config 기반 모바일 게임 Live Ops 실습 프로젝트

## Unity 버전
- Unity 6

## 필요 Firebase 패키지
- FirebaseApp
- FirebaseAnalytics
- FirebaseRemoteConfig

## Firebase SDK 다운로드
https://firebase.google.com/download/unity

## 포함 기능
- Firebase 초기화
- Firebase Analytics 이벤트 기록
- Firebase Remote Config 연동

- Login
- 가챠 UI Analytics
- Daily Login Reward

- 이벤트 ON/OFF 구조

- A/B Test 그룹 분기

- Red Dot 알림 시스템

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
