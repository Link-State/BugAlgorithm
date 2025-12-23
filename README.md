# 곤충 알고리즘
### [2025 2학기 로봇알고리즘 과제1]

### 개발 기간
> 2025.09.18 ~ 2024.09.24

### 개발 환경
> Unity 6000.2.1f1<br>
> RTX4050 Laptop<br>

### 설명
+ 동기
    + 로봇알고리즘 수업 과제
+ 기획
    + 곤충 1
      + Motion-to-goal : 목표 지점까지 직진 경로로 이동
      + Boundary-following : Motion-to-goal 중 장애물과 충돌 시 장애물의 외곽선을 따라 이동함. 이동하며 목표지점까지 가장 가까운 지점이 되는 좌표 q를 기억 후, 처음 충돌 지점에 도달하면 좌표 q로 이동한 후, 다시 Motion-to-goal 진행<br><br>
    + 곤충 2
      + 시작 지점부터 목표 지점까지 직선의 m-Line을 그린 후 Motion-to-goal 수행
      + Motion-to-goal : m-Line을 따라 이동함
      + Boundary-following : Motion-to-goal 중 장애물과 충돌 시 장애물의 외곽선을 따라 이동함. 이동하는 중 m-Line을 만나면 다시 Motion-to-goal 수행<br><br>
    + 탄젠트 곤충
      + 360˚를 감지하는 센서 장착 (간격은 5˚로 총 72개의 센서)
      + Motion-to-goal
        + 센서가 장애물을 감지하지 못한 경우, 목표 지점까지 직선 경로로 이동
        + 센서가 주변 장애물을 감지한 경우, 센서의 연속성(장애물을 감지한 센서의 부채꼴 영역)이 소실되는 지점까지의 거리 d1, d2, d3, ... dn에서 목표지점까지의 거리 g가 있을 때, min(d1 + g, d2 + g, ... dn + g)이 되는 d로 이동
      + Boundary-following : 목표 지점에 도달하지 않았으나 특정 시간동안 위치 변화가 0에 가깝다면 Localminimum에 빠졌다고 판단, Localminimum에서 목표 지점까지의 거리 d를 기억 후 장애물의 외곽선을 따라 이동하며 d보다 거리가 작아지는 지점에 도달하면 다시 Motion-to-goal 수행

#### 곤충 1

#### 곤충 2

#### 탄젠트 곤충

<br>

