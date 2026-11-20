# Shield Hero 기술 포트폴리오 초안

## 프로젝트 개요

* **개발 기간:** 2026년 9월 30일 \~ 11월 16일
* **형태:** 개인 프로젝트
* **장르 / 플랫폼:** 액션 플랫포머 / Windows
* **개발 환경:** Unity 6.3.17f1, C#, URP 2D
* **핵심 플레이:** 가드와 패리로 적을 상대하며, 무작위로 이어지는 방을 선택해 최종 보스를 처치
* **담당 범위:** 게임 시스템 설계 및 구현
* **시연 영상:** [Shield Hero 플레이 영상](https://youtu.be/UKcyqTSqLBw)

## 기술적으로 설명할 핵심 구현

### 1\. 가드·패리 중심 전투와 처형

* 플레이어 상태를 FSM으로 관리하고, 입력별 전환 조건은 전환 규칙 데이터로 분리.
* 가드 시작 시 패리 타이머를 시작하고, 제한 시간이 지나면 패리만 종료해 가드는 유지.
* 적 공격 정보에 체력 피해·스태미너 피해·가드/패리 가능 여부를 함께 담아 공격마다 방어 조건을 설정.
* 가드 시 스태미너를 먼저 차감하고, 이번 타격으로 스태미너가 파괴되면 가드 실패와 체력 피해로 연결.
* 적이 경직 상태일 때 주변 처형 가능 대상을 찾아 실행 위치로 이동한 뒤 특수 공격 수행.

**코드:** [PlayerCombat.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Player/PlayerCombat.cs), [PlayerStats.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Player/PlayerStats.cs), [PlayerTransitioner.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/FSM/PlayerTransitioner.cs), [PlayerExecuteState.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/FSM/PlayerStateMachine/PlayerExecuteState.cs), [EnemyAttackInfo.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Enemy/EnemyAttackInfo.cs)

### 2\. 무작위 경로를 공유 그래프로 구성하는 스테이지 생성

* 시작 노드에서 보스 노드까지 `pathCount`개의 경로를 생성하고, 경로마다 층을 따라 인접 라인으로 이동.
* 같은 층·라인에 도달한 경로는 하나의 노드를 공유하도록 Dictionary로 병합하고, 노드 간 연결은 다음 노드 ID로 저장.
* 엘리트는 지정 층의 중앙 라인에 배치하며, 방 클리어 후 현재 노드의 연결 정보로 갈 수 있는 방만 선택 가능.
* `floorCount`는 가로 진행 단계, `laneCount`는 각 단계의 세로 배치 범위, `pathCount`는 경로 생성 횟수. 경로가 합쳐지므로 `pathCount`가 실제 방 개수와 같지는 않음.
* 정해진 알고리즘 이름을 적용한 것이 아니라, 격자 경로 생성과 노드 병합을 조합한 프로젝트 자체 방식.

**코드:** [StageMapGenerator.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/System/Map/StageMapGenerator.cs), [StageMapNodeData.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Data/Room/StageMapNodeData.cs), [StageManager.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Singleton/StageManager.cs), [RoomSystem.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Singleton/RoomSystem.cs)

### 3\. 적 유형별 행동 구조

* 일반 적은 FSM으로 대기·순찰·추적·공격 상태를 전환.
* 엘리트와 보스는 Behavior Tree로 행동 후보를 평가하고, Selector와 Sequence로 우선순위 및 조건 검사를 구성.
* 공격별 사거리·쿨다운·피해·가드/패리 속성을 데이터로 분리하고, 보스는 페이즈별 행동 구성을 사용.

**코드:** [EnemyStateMachine](https://github.com/RFinex/Parry_Project_Clean/tree/main/Assets/01.Scripts/FSM/EnemyStateMachine), [EliteEnemyController.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Enemy/Controller/Elite/EliteEnemyController.cs), [BossController.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Enemy/Controller/Boss/BossController.cs), [EnemySkillData.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Data/EnemyData/SkillData/EnemySkillData.cs)

### 4. 유물 획득과 효과 적용

* 보물상자 데이터의 등급별 가중치로 보상 등급을 결정하고, 해당 등급에서 아직 드랍되지 않은 유물을 무작위로 선택.
* `ItemFactory`가 ID에 등록된 생성 함수로 유물 객체를 만들고, `ArtifactDataReader`에서 해당 ID의 ScriptableObject 데이터를 연결.
* 생성한 유물을 픽업 객체에 전달하고, 플레이어가 상호작용하면 런타임 인벤토리에 추가.
* 인벤토리에 추가된 유물은 `StatPair`를 플레이어 스탯에 적용하며, 유물 제거·교체 시 적용한 수치를 되돌림. 일부 유물은 장착 상태에서 이벤트를 구독해 조건부 효과를 처리.

**코드:** [TreasureObject.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/System/Interact/TreasureObject.cs), [ArtifactSystem.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/System/ArtifactSystem.cs), [ItemFactory.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Factory/ItemFactory.cs), [ItemPickup.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/System/Interact/ItemPickup.cs), [RuntimeInventorySystem.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/System/Inventory/RuntimeInventorySystem.cs), [Item.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Item/Item.cs)

### 5. 공격 판정의 GC Alloc 제거

* 매 공격마다 새 배열을 반환하던 `OverlapCircleAll`을 재사용 리스트를 받는 `OverlapCircle` 오버로드로 변경.
* 여러 콜라이더가 한 적을 감지해도 한 번만 피격되도록 적 단위 중복 검사를 추가.
* **측정 조건:** 더미 30마리를 같은 위치에 겹쳐 소환하고, 전후 동일한 공격 판정을 실행.
* **결과:** 해당 공격 판정 호출의 Profiler GC Alloc이 272 B에서 0 B로 감소.
* 측정한 항목은 호출당 GC Alloc이며, 전체 프레임 시간이나 게임 전체 성능 개선으로 확대 해석하지 않음.

#### 변경 전

```csharp
Collider2D\[] hits = Physics2D.OverlapCircleAll(
    attackPos, attackRange, data.EnemyLayer);

foreach (Collider2D hit in hits)
{
    EnemyController enemy = hit.GetComponentInParent<EnemyController>();
    if (enemy == null || enemy.Stats.IsDead || enemy.IsInvincible)
        continue;

    enemy.TakeDamage(attackDamage);
}
```

#### 변경 후

```csharp
hitColliders.Clear();
int hitCount = Physics2D.OverlapCircle(
    attackPos, attackRange, enemyFilter, hitColliders);

hitEnemyList.Clear();
for (int i = 0; i < hitCount; i++)
{
    Collider2D hit = hitColliders\[i];

    if (!hit.TryGetComponent<EnemyController>(out var enemy))
        enemy = hit.GetComponentInParent<EnemyController>();

    if (enemy == null || enemy.Stats.IsDead || enemy.IsInvincible
        || hitEnemyList.Contains(enemy))
        continue;

    float damage = isDamageConversion ? attackDamage \* 0.5f : attackDamage;
    PlayerAttackInfo info = new(damage, playerBoard.playerTransform);

    if (isDamageConversion)
        enemy.TakeBalanceDamage(info);
    else
        enemy.TakeDamage(info);

    hitEnemyList.Add(enemy);
}
```

`hitColliders`, `hitEnemyList`, `ContactFilter2D`를 재사용해 쿼리 결과 배열 할당을 제거.

**코드:** [PlayerCombat.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Player/PlayerCombat.cs)

## 문제 해결 사례

### 순찰 루프의 프레임 대기 누락

**문제:** 적이 벽이나 낭떠러지를 감지해 목표 위치를 바꾸는 상황에서 순찰 루프가 정상적으로 진행되지 않거나, 빠르게 반복될 수 있었음.

**원인:** 방향 전환 분기에서 `continue`를 실행하면 루프 하단의 `UniTask.NextFrame`에 도달하지 않음. 새 목표 위치에서도 같은 감지 조건이 반복되면 프레임을 양보하지 않고 루프가 연속 실행될 수 있었음.

#### 변경 전

```csharp
if (wallAhead || noGroundAhead)
{
    targetPos = owner.GetRandomPatrolPos(-direction);
    distanceX = targetPos.x - ownerPos.x;
    continue;
}

await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, ctk);
```

#### 변경 후

```csharp
if (wallAhead || noGroundAhead)
{
    targetPos = owner.GetRandomPatrolPos(-direction);
    distanceX = targetPos.x - ownerPos.x;
}

// 이동 처리 후 루프 하단의 프레임 대기에 도달
await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, ctk);
```

**해결:** `continue`를 제거하고 반대 방향의 목표 위치만 갱신하도록 수정. 이후 이동 처리를 거쳐 매 반복마다 FixedUpdate 프레임 대기에 도달하게 해 순찰 루프가 정상 동작하도록 함.

### 적 대기 중 타깃 탐지 지연

**문제:** 적이 대기 상태에 들어간 뒤 플레이어가 탐지 범위에 들어와도, 대기 시간이 끝날 때까지 추적 상태로 전환되지 않았음.

**원인:** 기존 흐름은 무작위 대기 시간을 한 번에 기다린 다음 타깃을 확인해, 대기 도중에는 탐지를 수행하지 않았음.

#### 변경 전

```csharp
float waitTime = UnityEngine.Random.Range(owner.Data.MinIdleTime, owner.Data.MaxIdleTime);
await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: ctk);

if (owner.TryDetectTarget())
{
    TransitionState(typeof(EnemyTraceState));
    return;
}
```

#### 변경 후

```csharp
float detectInterval = 0.1f;
float remainingIdleTime = UnityEngine.Random.Range(owner.Data.MinIdleTime, owner.Data.MaxIdleTime);

while (remainingIdleTime > 0f)
{
    float waitTime = detectInterval < remainingIdleTime ? detectInterval : remainingIdleTime;
    await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: ctk);
    remainingIdleTime -= waitTime;

    if (owner.TryDetectTarget())
    {
        TransitionState(typeof(EnemyTraceState));
        return;
    }
}
```

**해결:** 대기 시간을 짧은 구간으로 나누고 0.1초 간격으로 타깃을 확인하도록 변경. 대기 중에도 플레이어를 감지하면 추적 상태로 전환하도록 수정.

**코드:** [EnemyIdleState.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/FSM/EnemyStateMachine/EnemyIdleState.cs)

### 스태미너가 부족해도 대시 가능했던 문제

**문제:** 대시 쿨타임이 끝난 상태라면 스태미너가 부족하거나 스태미너 파괴 상태여도 대시 상태로 전환될 수 있었음.

**원인:** 대시 전환 규칙이 쿨타임만 검사하고, 스태미너 보유량과 파괴 상태는 확인하지 않았음.

#### 변경 전

```csharp
public override bool CanApply(PlayerBlackBoard board)
{
    return board.skill.dashCoolTimer <= 0f;
}
```

#### 변경 후

```csharp
public override bool CanApply(PlayerBlackBoard board)
{
    if (board.stats == null)
        return false;

    float staminaCost = board.stats.GetStats(StatId.DashStaminaCost);

    return board.skill.dashCoolTimer <= 0f
        \&\& !board.stats.IsStaminaBroken
        \&\& board.stats.NowStamina >= staminaCost;
}
```

**해결:** 대시 전환 조건에 스태미너 파괴 여부와 현재 보유량 검사를 추가. 대시 비용을 감당할 수 있고 스태미너 파괴 상태가 아닐 때만 상태 전환하도록 수정.

**코드:** [PlayerDashRule.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Data/PlayerData/PlayerStateTransitionRuleData/PlayerDashRule.cs)

### 엘리트 공격 애니메이션이 재생 중 초기화되는 문제

**문제:** 공격 애니메이션이 이미 재생 중인데 다시 재생 요청이 들어오면 애니메이션 진행도가 처음으로 돌아가 공격 동작이 반복되거나 타이밍이 어긋날 수 있었음.

**원인:** 애니메이터가 현재 재생 중인 상태인지 확인하지 않고 매번 시작 시간 `0f`로 `Play`를 호출했음.

#### 변경 전

```csharp
animator.Play(hash, 0, 0f);
return true;
```

#### 변경 후

```csharp
AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);

if (current.fullPathHash == hash)
{
    if (!animator.IsInTransition(0))
        return true;

    AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(0);
    if (next.fullPathHash == hash)
        return true;
}

animator.Play(hash, 0, 0f);
return true;
```

**해결:** 현재 상태와 전환 대상 상태가 이미 요청한 애니메이션인지 먼저 확인하고, 다른 상태일 때만 처음부터 재생하도록 수정. 재생 중복으로 진행도가 초기화되지 않게 했으며, 애니메이션 종료 판정도 전환 중인 다음 상태를 고려하도록 조정.

**코드:** [EnemyAnimator.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Enemy/EnemyAnimator.cs)

### 가드 파괴 후 체력 피해에서 피격 상태가 처리되지 않던 문제

**문제:** 가드 가능한 공격을 막는 과정에서 스태미너가 파괴되면 의도대로 체력 피해도 받아야 했지만, 피해 처리 경로가 일관되지 않아 체력만 감소하고 피격 상태 전환 및 피격 모션은 실행되지 않았음. 스태미너가 남아 있는 정상 가드에서는 체력 피해가 발생하지 않아야 했음.

**원인:** `PlayerStats`가 스태미너 파괴를 감지한 즉시 내부에서 `TakeDamage`를 호출했음. 이 호출은 플레이어의 일반 피해 처리 흐름인 `PlayerController.TakeDamage`를 거치지 않아 피격 상태와 애니메이션 처리를 건너뛰었음. 또한 가드 성공 여부와 스태미너 파괴 결과를 연결하는 책임이 분리되지 않아, 체력 피해를 적용할 시점을 가드 판정 쪽에서 명확히 제어하기 어려웠음.

#### 변경 전

```csharp
// PlayerStats 내부에서 스태미너 파괴 시 체력 피해까지 직접 처리
if (nowStamina <= 0f)
{
    TakeDamage(info.attackDamage);
    isStaminaBroken = true;
    nowStamina = 0f;
}
```

#### 변경 후

```csharp
// 가드 판정에서 먼저 스태미너 피해와 파괴 여부를 확인
stats.TakeStaminaDamage(info);

if (!stats.IsStaminaBroken)
    return true; // 스태미너가 남아 있으면 가드 성공

// 파괴된 공격만 일반 피격 경로로 전달
TakeDamage(info.attackDamage);
return false;
```

**해결:** `PlayerStats`에서는 스태미너 감소와 파괴 상태 갱신을 담당하고, 가드 판정 흐름에서 파괴 여부에 따라 후속 처리를 결정하도록 역할을 분리함. 스태미너가 남아 있으면 가드 성공으로 종료하고, 파괴된 경우에만 `PlayerController.TakeDamage`를 호출해 체력 피해와 피격 상태 전환·모션이 함께 처리되도록 수정함.

**코드:** [PlayerCombat.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Player/PlayerCombat.cs), [PlayerController.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Player/PlayerController.cs), [PlayerStats.cs](https://github.com/RFinex/Parry_Project_Clean/blob/main/Assets/01.Scripts/Player/PlayerStats.cs)

## 확인 자료

* Windows 실행 빌드 보유
* [플레이 영상](https://youtu.be/UKcyqTSqLBw)
* 코드 링크는 GitHub `main` 브랜치 기준
* 추가할 자료: 실행 빌드 링크, 실제 조작 안내, Profiler 캡처

