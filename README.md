# Project REINFORCED
![Imgur](https://i.imgur.com/mG01m8a.png)
Made by **MineEric64**, **abilitygamer06**

UI by **THEP0LLOCK** and **BullsEye**

**Helped by Nextop Coding Information Security Academy**

# Introduction
게이머들을 위한 하이라이트 캡처 프로그램입니다.

# API Reference
LCUSharp - 롤 클라이언트 API

[OpenCVSharp](https://github.com/shimat/opencvsharp) - OpenCV

# TODO
- [x] 롤 킬 이벤트 구현
- [ ] 레식 킬 이벤트 구현
- [ ] 킬 이벤트가 들어오면 녹화 저장
- [ ] 딥러닝을 통한 매드무비 자동 생성

# 계획
## 킬 이벤트 구현
- API로 현재 게임 플레이어 정보 요청 => 가져오기 => 이벤트
- Hook => 이벤트?

## 녹화
- 큐로 Mat 저장 => dequeue 반복하고 VideoWriter에 저장
