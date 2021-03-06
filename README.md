# Project REINFORCED
![Imgur](https://i.imgur.com/mG01m8a.png)
Made by **MineEric64** and **Juyong** (Team Prodigy)

UI by **THEP0LLOCK** and **BullsEye**

Helped by **Nextop Coding Information Security Academy**

# Introduction
게이머들을 위한 하이라이트 캡처 프로그램입니다.

초반에는 **Juyong**와 **THEP0LLOCK**이 아이디어를 내고 그 아이디어에 기반하여 **MineEric64**가 Project REINFORCED의 핵심 기능을 구현하였습니다.
이 프로젝트는 넥스탑 코딩 정보보안 학원에서 시작되었습니다.

## Words that developers want to tell
* 제작자 **MineEric64**: 솔직히 이번 프로젝트의 난이도는, 제 개인 프로젝트인 [유니컨버터](https://github.com/MineEric64/UniConverter-Project)에서
구현해야 하는 기능이 있는데 그 기능을 구현하려면 미분을 배워야 하는 난감한 상황 이후로 2번째로 어려웠습니다. 유니컨버터를 만들 때 당시 저는 중2였기 때문에
연립방정식 문제를 푸는 것도 어려운데 미분을 배우려면 얼마나 더 어렵겠습니까,

아무튼 이 프로젝트를 만들면서 처음으로는 게임 이벤트를 구현해야할 때 정말 난감했습니다. 심지어 구글링을 해도 나오는 자료가 없으니깐 저의 뇌피셜만으로
게임 이벤트 기능을 구현해야했었습니다. 처음에는 게임을 후킹하여 메모리에서 변수를 읽고 이벤트를 가져오는 줄 알았는데, 아니였던 것 같습니다. (확실하지 않음)
그래서 리그 오브 레전드라는 게임같은 경우에는 구글링만 1주 동안 해서 영어로 된 API 문서를 읽으니 결국 게임 이벤트 가져오는 것을 구현하는 데에 성공했습니다.
> 리그 오브 레전드의 게임 이벤트 기능 관련 코드는 [여기](../../blob/main/Clients/Lol/LolClient.cs)에서 찾아보실 수 있습니다.

2번째로는 화면을 녹화할 때입니다. 처음에는 제가 원래 알고 있었던 ```Graphics.CopyFromScreen()``` 코드를 이용하여 화면을 캡처한 뒤, 무한 반복을 해서
화면을 녹화하려고 했는데 OpenCV에서는 동영상 저장을 할 때 동영상의 사운드를 저장할 수 없다는 한계에 난감했습니다. 그리고 ```Graphics.CopyFromScreen()``` 함수를 쓸 때
30fps 이상을 녹화하려면 더 빨리 화면을 캡처할 수 있는 코드나 라이브러리가 필요하였습니다. 이 것도 2주동안 구글링을 하고 테스트의 반복을 하여 시행착오 끝에,
Desktop Duplication API를 이용하여 아주 빠른 화면 캡처를 할 수 있게 되었습니다. 동영상의 사운드를 저장하는 것은 사운드를 개별로 분리하여 녹음을 하고
동영상을 저장할 때 녹화한 동영상과 녹음을 한 사운드를 합쳐서 최종 동영상이 나오게 할 것 같습니다.
> 녹화 기능 관련 코드는 [여기](../../blob/main/Recording/Screen.cs)에서 찾아보실 수 있습니다.

감사합니다!

# API Reference
## 게임 클라이언트
|API 이름|설명|참조|
|:---:|:---:|:---:|
|[LCUSharp](https://github.com/bryanhitc/lcu-sharp)|롤 클라이언트 API|[LolClient.cs](../../blob/main/Clients/Lol/LolClient.cs)|

## 화면 녹화
|API 이름|설명|참조|
|:---:|:---:|:---:|
|[OpenCVSharp](https://github.com/shimat/opencvsharp)|OpenCV|[Screen.cs](../../blob/main/Recording/Screen.cs#L186)|
|[desktop-duplication-net](https://github.com/jasonpang/desktop-duplication-net)|화면 캡처|[Screen.cs](../../blob/main/Recording/Screen.cs#L442)|
|[MessagePack-CSharp](https://github.com/neuecc/MessagePack-CSharp)|데이터 압축|[ScreenCaptured.cs](../../blob/main/Recording/Types/ScreenCaptured.cs)|

## 소리 녹음
|API 이름|설명|참조|
|:---:|:---:|:---:|
|[NAudio](https://github.com/naudio/NAudio)|[사운드 장치 캡처 (녹음) 및 소리 합병](../../issues/3)|[Audio.cs](../../blob/main/Recording/Audio.cs)|
|[NAudio.Lame](https://github.com/Corey-M/NAudio.Lame)|소리 파일 저장|[Audio.cs](../../blob/main/Recording/Audio.cs#L203)|
|[FFmpeg](https://ffmpeg.org/)|[영상 및 소리 합병](../../issues/5)|[Screen.cs](../../blob/main/Recording/Screen.cs#L472)|

# TODO
- [x] 롤 킬 이벤트 구현
- [ ] 레식 킬 이벤트 구현
- [x] 킬 이벤트가 들어오면 녹화 저장
- [ ] 딥러닝을 통한 매드무비 자동 생성

# 계획
## 킬 이벤트 구현
- API로 현재 게임 플레이어 정보 요청 => 가져오기 => 이벤트

## 녹화
- Desktop Duplication API를 이용한 스크린샷 저장
- 스크린샷 => 데이터 변환 (Bitmap -> byte[]) => [MessagePack](https://github.com/neuecc/MessagePack-CSharp)으로 데이터 압축 및 Serialize => 큐로 [ScreenCaptured](../../blob/main/Recording/Types/ScreenCaptured.cs) 저장 => pop 반복하고 데이터 압축 해제 및 Deserialize => VideoWriter에 저장
  - [#6](../../issues/6) 참고

## 녹음
- NAudio의 WasapiCapture 및 WasapiLoopbackCapture를 통해 사운드 캡처 => 큐로 데이터 저장 => pop 반복 => `MergeMp3()` 함수를 통한 소리 합병
  - [#3](../../issues/3), [Audio.cs](../../blob/main/Recording/Audio.cs) 참고

## 동영상 저장

