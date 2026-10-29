# 콩이의 모험 (Re:KONG) — Claude Code 안내서

이 파일은 claude.ai 채팅에서 진행한 설계·결정 사항을 정리한 것이다. (최종 갱신: 2026-10-29)
**실제 코드와 이 문서가 다르면 실제 코드가 기준**이다. 작업 전에 관련 파일을 직접 읽고 확인할 것.

---

## 1. 프로젝트 개요

- 유니티 6 (에디터 6000.6.0f1) 기반 **2D 탑뷰 싱글플레이 액션 RPG**
- 말티즈 콩이가 사후세계를 탈출해 환생하는 이야기. 세상을 떠난 반려견 콩이를 추모하려는 개인 프로젝트
- 핵심 재미: **쉽고 화려한 액션(패리 중심) + 귀여운 아트**. 소울라이크 시스템을 가져오되 초보자도 쉽게 즐기는 방향
- "패리"와 "반격"은 같은 뜻
- 16주 학기 프로젝트, 매주 진행 상황 발표 있음
- 맵은 2D 유지 (2.5D 전환 안 함). 기간이 부족하면 맵 축소·생략, 스토리 단축
- 개발 환경: 코드 편집기 Rider, 주로 맥·아이패드
- 버전 관리: GitHub (사용자는 커밋·푸시·풀만 해봄, 되돌리기 경험 없음)

## 2. 사용자와 응답 규칙 (중요)

- 사용자는 **컴퓨터공학과 대학생, 유니티 초보**. 유니티 공부와 게임 개발을 동시에 하고 있음
- **한국어로 답할 것.** 초보자 눈높이로 차근차근 설명
- **코드를 수정·추가할 때는 항상 파일 전체 코드를 제공**하고, 이전 버전에서 바뀐 줄에 `// [추가]`, `// [수정]`, `// [삭제]` 주석을 달 것
  - (Claude Code가 파일을 직접 고칠 때도, 무엇이 바뀌었는지 같은 형식으로 설명할 것)
- 코드를 바꿀 때는 항상 다음을 함께 설명:
  - **무엇을** 바꾸는지 (파일, 함수, 위치)
  - **왜** 바꾸는지 (지금 방식의 문제, 안 바꾸면 생기는 일, 어떤 게 더 좋은지)
  - **어떻게** 동작하는지
  - **순서·위치가 중요한 경우** 다른 곳에 넣으면 무슨 일이 생기는지
- 처음 나오는 문법·개념은 짧게 풀어서 설명
- 입력은 반드시 **새 Input System + 기존 `PlayerInputHandler`** 를 통해 받을 것. `Input.GetKeyDown` 같은 구 Input 금지
- 새 `[SerializeField]` 칸이 생기면 **인스펙터에서 무엇을 연결해야 하는지 반드시 안내**할 것
- 발표자료 슬라이드 문구는 '-했어요/-했습니다' 대신 **'-함/-했음'처럼 짧게 끊는 어투**. 전문 용어를 피하고 처음 보는 사람도 이해하게

## 3. 작업 규칙

- 큰 수정 전에 사용자에게 **현재 상태를 커밋했는지 확인**할 것
- `.unity`, `.prefab`, `.asset` 파일은 직접 편집하지 말고, **유니티 에디터에서 할 작업으로 안내**할 것
- 파일 이동·이름 변경·삭제는 **유니티 Project 창에서** 하도록 안내 (`.meta` 유지)
- 스크립트를 지울 때: 씬의 컴포넌트를 먼저 제거하거나, 지운 뒤 남은 "Missing (Mono Script)"를 제거하도록 안내
- 커밋 전 **씬 저장(Ctrl+S)** 을 상기시킬 것 (씬 저장 전 커밋으로 연결이 빠진 적이 있음)
- 컴파일 에러가 하나라도 있으면 새 코드가 반영되지 않으므로, 에디터 작업 전에 **콘솔의 `error CS`부터 해결**하도록 안내
- `RequireComponent`는 이미 붙어 있는 컴포넌트에는 소급 적용되지 않음 → 필요한 컴포넌트는 직접 추가하라고 안내
- 테스트용으로 바꾼 SO 값(최대 체력 등)은 플레이를 꺼도 유지되므로 원래대로 돌리라고 안내
- **사용자가 시킨 작업이 끝나면 항상 13장 "작업 기록"에 추가할 것** (진행 상황을 기억하기 위함)
  - ① 어떤 작업을 했는지 ② 이전과 달라진 점 — 추가 / 수정 / 삭제로 나눠서
  - 코드만 고친 것과, 사용자가 에디터에서 해야 하는 일(씬·컴포넌트 설정)은 구분하고, 테스트 여부도 적을 것
  - 구조가 바뀌면 6장(스크립트 구조)·12장(폴더 구조) 같은 본문도 함께 고칠 것
- 이 파일의 원본은 **프로젝트 폴더(`SW_Project/CLAUDE.md`)** 이다. 구글 드라이브 사본은 자동으로 바뀌지 않음

## 4. 설계 원칙

1. **한 스크립트는 한 가지 책임.** 나누는 기준은 "기능 최소 단위"가 아니라 **"바뀌는 이유가 같은가"**
   - 나눌 신호: 역할 설명에 "그리고"가 자꾸 들어감, 읽었을 때 어색한 호출 (예: `stamina.BeginInvincible()`)
   - 묶을 신호: A를 고칠 때 B도 거의 항상 고침, 같은 값을 둘 다 바꿈
   - 너무 쪼갠 신호: 나눴더니 둘이 서로를 계속 불러야 함
2. **값과 규칙은 한 곳에서만 관리.** 같은 값을 두 스크립트가 바꾸면 안 됨 (적 색은 `EnemyVisual`만, 행동 규칙은 `PlayerStateController.CanEnter()`만)
3. **대답이 필요하면 직접 호출, 알리기만 하면 이벤트.** UI·연출은 이벤트를 구독해서 반응
4. **연결이 빠지면 크게 알린다.** `Awake`에서 null 검사 → `Debug.LogError` + `enabled = false`
5. 이벤트 구독은 `OnEnable`, 해제는 `OnDisable`
6. **같은 오브젝트의 컴포넌트는 자동 연결** (`[RequireComponent]` + `Awake`에서 `GetComponent`). 인스펙터 칸은 다른 오브젝트·자식·에셋·프리팹만
7. 수치는 ScriptableObject, 동작은 MonoBehaviour. 다른 스크립트는 수치를 `PlayerStats`에서만 읽음
8. 행동은 `TryEnter`로 **허락받고** 시작, 끝나면 `Exit`. 끊긴 행동은 `StateChanged`를 듣고 **스스로 정리** (다른 행동을 직접 취소하지 않음)
9. "막을 거면 아무것도 하기 전에 막는다" — 검사(return)는 상태를 바꾸는 줄보다 위에

## 5. 입력 (PlayerControls.inputactions, Player 맵)

| 행동 | 키 | 게임패드(예정) |
|---|---|---|
| 이동 | WASD | 왼쪽 스틱 |
| 공격 | 마우스 좌클릭 | X (West) |
| 대쉬(회피) | 마우스 우클릭 | |
| 패리 | 스페이스바 | |
| 스킬 | **미정** (E 제안) | |

`PlayerInputHandler`가 `Move`, `AttackPressed`, `DashPressed`, `ParryPressed`를 제공. 각 행동 스크립트가 `Update()`에서 매 프레임 물어봄 (`WasPressedThisFrame` 기반).

## 6. 스크립트 구조

### 흐름
```
입력(InputHandler) → 허락(StateController) → 행동(Movement/Attack/Parry)
→ 자원(Health/Stamina, 수치는 Stats) → 방송 → 반응(HitReaction/Invincibility) · UI · 연출
```
위층은 아래층을 직접 부르고, 아래층은 방송만 한다.

### Player 오브젝트 (Tag: Player)
| 스크립트 | 역할 | 자동으로 찾는 것 | 인스펙터 칸 |
|---|---|---|---|
| `PlayerInputHandler` | 키 입력 창구 | | |
| `PlayerStateController` | 현재 상태 + 행동 규칙표. `CanEnter`/`TryEnter`/`Exit`, `StateChanged` 이벤트 | | Log State Changes |
| `PlayerStats` | 최종 수치 제공 (장비 보너스가 붙을 자리). `IsValid` | | PlayerStatsData |
| `PlayerHealth` | 체력, 피격 입구(`TakeDamage`, `IDamageable`), 무적 검사(요청 개수 카운트), 사망. 이벤트: `OnHealthChanged`, `OnDamaged`, `OnDeath` | Stats | |
| `PlayerStamina` | 스태미나 `TryConsume`/`Recover`, 자연 회복. 이벤트: `OnStaminaChanged`, `OnStaminaExhausted` | Stats | |
| `PlayerInvincibility` | "어떤 상태일 때 무적인가" 규칙. `StateChanged`를 듣고 Health에 `BeginInvincible`/`EndInvincible` | StateController, Health | 무적 상태 목록 (기본: Dashing) |
| `PlayerMovementSmooth` | 걷기·대쉬 (둘 다 속도를 바꾸므로 한 스크립트). 무적은 모름 | Input, StateController, Stats, Stamina | 이동·대쉬 수치 |
| `PlayerAttack` | 선딜→판정→후딜, 선입력. 데미지 = `Stats.BaseAttack` + 무기. `HitLanded` 이벤트 | Input, StateController, Stats | Hitbox, Animator, Weapon |
| `PlayerParry` | 패리 자세 + 타이밍 판정(`Judge`), 성공 시 `Stamina.Recover(Stats.ParryPayback)`. `ParrySucceeded` 이벤트 | Input, StateController, Stats, Stamina | Animator, 타이밍 |
| `PlayerHitReaction` | 맞으면 경직, 체력 0이면 사망 상태 | Health, StateController | 경직 시간 |
| `StaminaFailFeedback` | 스태미나 부족 시 Visual만 떨기 | Stamina | Shake Target(Visual), 사운드 |
| `DamagePopupSpawner` | `HitLanded` → 맞은 위치에 숫자 | Attack | DamagePopup 프리팹 |
| `ParryPopupSpawner` | `ParrySucceeded` → 머리 위에 "패리"/"퍼펙트 패리!" | Parry | ParryPopup 프리팹(한글 폰트), 문구·색 |
| `ParryCameraShake` | `ParrySucceeded` → Cinemachine Impulse로 카메라 흔들기 (퍼펙트는 더 세게) | Parry, CinemachineImpulseSource | Normal Force(0.3), Perfect Force(0.8) |

플레이어 자식: `Visual`(콩이 스프라이트, 떨림 대상 — Rigidbody 있는 본체는 절대 흔들지 말 것), 무기(개껌 스프라이트 + 공격 판정 BoxCollider2D)

### Canvas (UI) — Player를 인스펙터로 연결
| 스크립트 | 역할 |
|---|---|
| `PlayerStatusUI` | 체력바·스태미나바 (Image Type = Filled). Health/Stamina 칸, 비면 태그로 검색 |
| `StaminaWarningUI` | 부족 순간의 플레이어 위치에 경고 문구, 1초간 재출력 금지. PlayerStamina 칸 |

### 적
| 스크립트 | 역할 |
|---|---|
| `Enemy` | 데이터·체력·사망 (`IDamageable`). 이벤트: `Damaged`, `Died`. 사망 시 콜라이더 끄고 잠시 후 Destroy |
| `EnemyAttacker` | 대기→예고→적중→후딜 루프. 적중 순간 `PlayerParry.Judge()`, 실패면 `PlayerHealth.TakeDamage()`. 플레이어는 태그로 찾음. 플레이어 사망·자신 사망 시 정지 |
| `EnemyVisual` | **적 그림 색의 유일한 담당.** 상태 색 + 피격 번쩍임(덮어쓰기) + 사망 색. 실제 색 변경은 `Apply()` 한 곳 |

### 데이터 (ScriptableObject)
| 파일 | 내용 |
|---|---|
| `PlayerStatsData` | 최대 체력·스태미나, 기본 공격력, 대쉬/패리 소모, 패리 페이백, 회복 속도·지연 |
| `WeaponData` | 무기 이름, 무기 공격력 |
| `EnemyData` | 몬스터 이름, 최대 체력, 공격 목록(순서대로 반복), 공격 간격 |
| `EnemyAttackData` | 공격 이름, 데미지, 패리 가능 여부(bool), 선딜, 후딜 |

### 공용
`IDamageable`, `ParryResult`(None/Normal/Perfect), `DamagePopup`(떠오르는 글자, `Setup(float)` 숫자용 / `Setup(string, Color)` 글자용)

### 정리 대상 (사용 안 함)
`IParryable`(파일명 오타 `IPlarryable.cs`), `TrainingDummy` → 오브젝트/컴포넌트 먼저 제거 후 스크립트 삭제

## 7. 행동 규칙표 (`PlayerStateController.CanEnter`)

| 지금 상태 | 공격 | 대쉬 | 패리 |
|---|---|---|---|
| 대기 | O | O | O |
| 공격 중 | O (선입력) | O (공격 취소) | O (공격 취소) |
| 대쉬 중 | X | X | X |
| 패리 중 | X | O (패리 취소) | X |
| 피격 경직 중 | X | O | O |
| 사망 | X | X | X |

- 피격: 사망 외 모든 상태에서 경직 (대쉬 중엔 무적이라 안 맞음)
- 경직·사망 중엔 걸어서 이동 불가
- `Exit(state)`는 현재 상태가 그 상태일 때만 대기로 돌아감

## 8. 주요 수치 (임시값 포함)

- 체력 100, 스태미나 100 / 대쉬 15, 패리 20 / 패리 성공 페이백 30 (1회) / 1초 미사용 후 초당 40 회복
- 패리: 입력 후 0.1초 안 적중 = 퍼펙트, 0.3초 안 = 일반. 헛패리 쿨타임 0.2초, 성공 시 쿨타임 없음
- 대쉬 0.22초, **대쉬 시간 동안만 무적**
- 피격 경직 0.3초, 퍼펙트 패리 시 적 그로기 1.2초, 적 사망 후 0.5초 뒤 삭제
- 적 색: 노랑 = 패리 가능 예고, 보라 = 패리 불가 예고, 파랑 = 그로기, 빨강 = 피격, 회색 = 사망
- 일반 공격은 스태미나를 쓰지 않음

## 9. 다음 작업 (권장 순서)

1. **카메라 흔들림**: Cinemachine 설치, Impulse로 패리 성공 시 흔들기 (따라가기·맵 경계·줌도 Cinemachine으로)
   - 10/29: 설치·패리 흔들림 코드 완료(테스트 미확인). 검토 중: 패리 성공 시 슬로우 모션, 카메라 확대
2. **스킬 (최소 버전)**: 아래 기본값으로 먼저 동작시키고 조절
3. **애니메이션 (병행)**: 콩이 Idle 하나로 도트 → 스프라이트 시트 → Animator 연결 한 바퀴 먼저

### 스킬 설계
- 확정 방향: **스택형**. 패리 성공 +1, 회피 +0.5 스택. 스택만큼 강해짐. 스킬도 스태미나 소모
- 제안 기본값 (사용자 확정 전):

| 항목 | 제안 |
|---|---|
| 단축키 | E |
| 회피 스택 | 회피로 공격을 피했을 때 +0.5 |
| 최대 스택 | 3 |
| 사용 조건 | 스택 1 이상 + 스태미나 30 |
| 효과 | 강한 일격, 데미지 = 기본 공격력 × (1 + 스택 × 0.5) |
| 사용 후 | 스택 0 |

- 구현 시: `PlayerState`에 스킬 상태 추가 → `CanEnter` 규칙 결정 필요, 입력 액션 추가, 스택은 별도 스크립트가 `ParrySucceeded`·회피 성공을 듣고 관리

### 그 외
- 적 공격 범위 + 범위 시각화
- 플레이어 사망 후 **가장 최근 저장 데이터 불러오기** (저장 시스템 생긴 뒤, `PlayerHitReaction.OnDeath()`에 연결)
- UI 연출 DOTween, 분위기는 2D 조명·후처리·파티클
- 장비 시스템 (무기 1개 = 공격력, 방어구 1개 = 체력) → `PlayerStats`에서 계산

## 10. 알려진 구조 개선점

| 항목 | 이유 | 시점 |
|---|---|---|
| 이동·대쉬 분리 | 대쉬에 기능(충전, 대쉬 공격 등)이 붙으면 바뀌는 이유가 달라짐 | 대쉬 확장 시 |
| 애니메이션 담당 스크립트 | Attack·Parry가 같은 Animator에 각자 `Play("Idle")` | 애니메이션 작업 전 |
| 적 판단·실행 분리 | 보스 패턴이 들어오면 EnemyAttacker가 복잡해짐 | 보스 작업 시 |
| 애니메이터 상태 이름 모으기 | "Idle", "Attack", "Parry"가 여러 스크립트에 흩어짐 | 애니메이션 작업 시 |
| `DamagePopup` → `FloatingText` | 숫자·글씨 둘 다 띄움 | 여유 있을 때 |
| 안 쓰는 코드 정리 | IParryable, TrainingDummy | 아무 때나 |

## 11. 아트

- 콩이 도트: 128x128, 사람처럼 서 있는 자세, 검정 테두리, 지나치게 사실적이지 않은 2D 도트. 이동·공격·회피·패리 동작 필요
- 무기: 개껌 (1024x1024 원본, PPU로 크기 조절)
- 이미지 설정: 도트는 Filter Mode = Point, Compression = None. 크기는 Scale이 아니라 PPU로
- 2D 기준은 "기본 상태 = 오른쪽"

## 12. 권장 폴더 구조

```
Assets/
├── Scripts/
│   ├── Input/        PlayerControls.cs, PlayerInputHandler.cs
│   ├── Player/       PlayerStateController, PlayerStats, PlayerHealth, PlayerStamina,
│   │   │             PlayerInvincibility, PlayerMovementSmooth, PlayerAttack,
│   │   │             PlayerParry, PlayerHitReaction
│   │   └── Feedback/ StaminaFailFeedback
│   ├── Enemy/        Enemy, EnemyAttacker, EnemyVisual
│   ├── Combat/       IDamageable, ParryResult
│   │   └── Feedback/ DamagePopup, DamagePopupSpawner, ParryPopupSpawner, ParryCameraShake
│   │                 (현재 실제 위치는 Combat/ 바로 아래)
│   ├── UI/           PlayerStatusUI, StaminaWarningUI
│   └── Data/         PlayerStatsData, WeaponData, EnemyData, EnemyAttackData
├── Data/             SO 에셋 (Weapons/, Enemies/, EnemyAttacks/)
├── Prefabs/          DamagePopup, ParryPopup
├── Input/            PlayerControls.inputactions
├── Sprites/
└── Fonts/            한글 폰트 SDF (TextMeshPro용)
```

## 13. 작업 기록

작업이 끝날 때마다 **맨 위에** 새 항목을 추가한다 (최신이 위). 형식: 날짜 — 제목 / 한 일 / 추가·수정·삭제 / 상태.

### 2026-10-29 — 패리 성공 시 카메라 흔들림
- **한 일**: Cinemachine 설치(사용자, 커밋 `4dd8106`), 패리 성공 시 카메라를 흔드는 스크립트 작성(Claude Code)
- **추가 (코드)**: `Scripts/Combat/ParryCameraShake.cs` — `PlayerParry.ParrySucceeded`를 듣고 `CinemachineImpulseSource.GenerateImpulseWithForce()` 호출. 일반 0.3 / 퍼펙트 0.8. Player에 붙이면 Impulse Source 자동 추가
- **추가 (패키지)**: Cinemachine **6.6.0** (`com.unity.cinemachine`, 네임스페이스 `Unity.Cinemachine`, Cinemachine 3 계열 API)
- **에디터 작업 (안내함)**: `GameObject > Cinemachine > Targeted Cameras > 2D Camera`로 CinemachineCamera 생성(Main Camera에 CinemachineBrain 자동 추가) → CinemachineCamera에 Add Extension `Cinemachine Impulse Listener` → Player에 `ParryCameraShake` 추가 (Impulse Shape Bump, Duration 0.2 권장)
- **수정 / 삭제**: 없음 (기존 스크립트는 그대로)
- **상태**: 플레이 테스트 결과 미확인. 씬 변경(`SampleScene.unity`)은 아직 커밋 전

### 2026-10-28 — CLAUDE.md 갱신·프로젝트 폴더로 이동 (`17fe26e`)
- **한 일**: 채팅 설계를 새 지침으로 정리, 구글 드라이브에서 `SW_Project/CLAUDE.md`로 복사해 Git에 추가
- **변경**: 예전 "8. 적용 상태 주의" 항목 삭제(아래 10/27·10/28 커밋으로 반영됨), 설계 원칙·작업 규칙 추가, 다음 작업을 Cinemachine·스킬·애니메이션으로 정리

### 2026-10-28 — 구조 변경 (`b16f43f`, 채팅 코드 적용)
- **추가**: `PlayerStats`(최종 수치), `PlayerHealth`, `PlayerStamina`, `PlayerInvincibility`(대쉬 중 무적 규칙), `ParryPopupSpawner`, `Prefab/ParryPopup.prefab`
- **삭제**: `PlayerHealthStamina` → Stats / Health / Stamina 세 개로 분리
- **수정**: `PlayerAttack`, `PlayerMovementSmooth`, `PlayerParry`, `PlayerHitReaction`, `StaminaFailFeedback`, `PlayerStatusUI`, `StaminaWarningUI`, `DamagePopup`, `DamagePopupSpawner`, `EnemyAttacker` — 새 스크립트를 쓰도록 바꾸고, 같은 오브젝트 컴포넌트는 자동 연결

### 2026-10-27 — 플레이어 상태 관리, 피격 경직, 몬스터 사망 (`1d53d8c`, 채팅 코드 적용)
- **추가**: `PlayerStateController`(행동 규칙표), `PlayerHitReaction`(경직·사망 상태), `EnemyVisual`(적 색 담당)
- **삭제**: 옛 `Scripts/PlayerMovement.cs`, `PlayerAttack`의 `InputLocked`와 공개 `CancelAttack` (→ `TryEnter`/`StateChanged` 방식)
- **수정**: `Enemy`(사망 처리), `EnemyAttacker`, `PlayerAttack`, `PlayerMovementSmooth`, `PlayerParry`
- **이동**: `Enemy`·`EnemyAttacker`·`EnemyData` → `Scripts/Enemy/`, `EnemyAttackData` → `Scripts/Data/`, `ParryResult` → `Scripts/Player/`
- 같은 날 Claude Code로 예전 지침의 "적용 상태 주의" 항목을 점검 → 당시 코드에 미반영 상태였음을 확인한 뒤 이 커밋으로 반영

### 그 이전 (커밋 기록 요약)
- 09/24~09/28: 공격(선딜·판정·후딜), 이동·대쉬·패리 첫 버전, 애니메이터 수정
- 10/01~10/02: 체력·스태미나, 콩이·개껌 이미지, 데미지 팝업, 공격력, 무기 오브젝트
- 10/05: 패리 테스트 (`261005_ParryTest`)
