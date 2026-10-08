# Parry Project - Codex Project Guide

## Project Overview
- Unity 2D side-view roguelike action game.
- Core combat identity: parry-centered combat.
- Normal attacks remain viable; successful parries provide the main high-reward loop.
- Successful parry breaks enemy balance and enters EnemyStaggerState.
- Execution is possible against staggered enemies. Player enters an execution state, approaches the enemy's execution position (preferably behind), then executes.
- Normal/elites can be killed by execution; bosses can instead receive large damage.
- Relics/items are run-only.
- Lobby permanent upgrades use saved gold and can improve attack power, speed, and parry judgement.
- Death currently has little/no major permanent penalty beyond losing run-only items; persistent gold is retained.

## Development Rules
- Prefer concise, code-first communication.
- When code changes are requested, provide the complete updated code when practical, not isolated snippets.
- Clearly mark modified areas with comments such as `// [수정]`.
- Treat the latest user-provided code as the current baseline.
- Do not unnecessarily redesign or replace existing architecture.
- Avoid overengineering.
- Prefer low coupling and clear responsibility separation.
- Avoid `Find` / `FindFirstObjectByType` for UI references when registry/reference approaches work.
- Avoid large hardcoded `if`/`switch` routing when extensible dictionaries, registries, or events are appropriate.
- Do not change Unity Script Execution Order to solve initialization issues.
- Do not introduce async-only initialization just to avoid normal `Awake` initialization.
- States are created once by the StateMachine. Do not instantiate new state objects directly inside controllers.
- Keep the FSM simple enough for the current project; do not overbuild it.

## Player
Current player features:
- Left/right movement
- Jump
- Dash with temporary invulnerability
- Guard/parry
- Attack
- No double jump unless explicitly requested later

PlayerController uses Rigidbody2D, PlayerBaseData, PlayerStats, StateMachine<PlayerController>, MoveInput/frontDir, Unity Input System callbacks, and `rb.linearVelocity`.
Ground checking uses BoxCast.
Dash uses UniTask and cancellation tokens.

Player FSM currently includes states such as Idle, Attack, Guard, Dash, and Jump.
Do not create states with `new PlayerJumpState()` etc. inside PlayerController.
States are registered once and reused.
Use state checks/input gating rather than unnecessary per-state external event subscriptions.

## Generic StateMachine
Current direction:
- `StateMachine<T>` caches states in `Dictionary<Type, BaseState<T>>`.
- `AddState<TState>()` creates a state once.
- `GetState<TState>()` retrieves the existing instance.
- `IsState(Type)` checks the current state.
- `ChangeState(Type)` exits the current state, manages transition subscription, and enters the registered state.

BaseState:
- Does not own a general Update/FixedUpdate.
- States start their own UniTask loop from Enter when needed.
- Token handling belongs inside Enter/Exit.
- Controller owns the lifecycle token.
- States create linked tokens from the controller token.

Important token pattern:
```csharp
protected void EnterToken(CancellationToken ownerToken)
{
    token?.Cancel();
    token?.Dispose();
    token = CancellationTokenSource.CreateLinkedTokenSource(ownerToken);
}

protected void ExitToken()
{
    token?.Cancel();
    token?.Dispose();
    token = null;
}
```

Do not remove controller token initialization merely to solve state initialization concerns.

## Enemy
Current enemy states:
- EnemyIdleState
- EnemyDeadState
- EnemyAttackState
- EnemyPatrolState
- EnemyTraceState
- EnemyStaggerState

Detection:
- EnemyController uses Raycast target detection.
- Do not make it depend directly on a Player Transform.
- `TryDetectTarget()` uses FrontDir and DetectRange, checks playerLayer/obstacleLayer, and stores a generic Transform.
- DetectRange and TraceRange are intentionally separate.
- Keep `IsTargetInAttackRange()`, `IsTargetInDetectRange()`, and `IsTargetInTraceRange()` responsibilities separate.

EnemyStats contains HP/balance state, stagger duration, damage, balance damage, balance restoration, `IsDead`, and `IsBalanceBroken`.
Balance break transitions the enemy to EnemyStaggerState.
EnemyStaggerState restores balance over StaggerDuration using a UniTask and then returns to EnemyIdleState.

## Execution
Current interface:
```csharp
public interface IExecuteTarget
{
    bool CanExecute { get; }
    Vector2 GetExecutePos(Vector2 attackerPos);
    void Execute();
}
```

Execution rules:
- Enemy can execute only while staggered.
- Prefer an explicit child ExecutePoint instead of changing the sprite pivot.
- PlayerExecuteState moves toward the execution position.
- Enemy.Execute() handles the result.
- Normal enemies/elites can die from execution; bosses may override behavior to take large damage.

## Cancellation / Lifecycle
EnemyController owns a CancellationTokenSource.
- `Initialized()` creates the controller token before the first state is entered because states call `EnterToken(owner.DestroyToken)`.
- OnDisable cancels/disposes/nulls the token.
- OnEnable recreates it when null and starts the controller UniTask.
- Pooling can reuse this lifecycle approach.
- Each state creates a linked token on Enter and cancels/disposes it on Exit.
Do not move token creation out of the current initialization flow just to avoid a null reference.

## Object Pooling
Architecture:
- ObjectPoolManager owns the global default `poolSize`.
- PoolDataBase contains registration data such as ID and prefab.
- PoolInfo should not own pool size.
- ObjectPool<T> keeps:
  - `Dictionary<int, Queue<T>> poolDic`
  - `Dictionary<int, Transform> parentDic`
- `poolDic`: object ID -> reusable queue.
- `parentDic`: object ID -> storage/return parent.
- The `parentDic` transform is NOT the usage parent. It is the object's home/storage parent.

Lifecycle:
```text
ObjectPoolParent
└── Pool_1
    ├── Object
    ├── Object
    └── Object

Get
Pool_1/Object -> actual usage parent

Return
actual usage parent/Object -> Pool_1/Object
```

`Get(objectId, position, parent)` receives the actual usage parent:
- World object: `parent = null`
- UI object: `parent = canvasTransform`

Do not move an object to `parentDic` during Get and then immediately move it again. Queued objects are already under their pool parent.

Conceptual Get:
```csharp
item = queue.Count > 0 ? queue.Dequeue() : Create(objectId);
item.transform.SetParent(parent, false);
item.transform.position = position;
item.gameObject.SetActive(true);
item.InitPool();
```

Conceptual Return:
```csharp
target.InitPoolReturn();
target.transform.SetParent(poolParent, false);
target.gameObject.SetActive(false);
queue.Enqueue(target);
```

Dynamic expansion:
- Prewarm the global default pool size at registration.
- If a queue is empty, Factory.Create() one additional object.
- The new object joins the same pool when returned.
- Callers do not pass pool size.

Pool roots:
- General object pools use ObjectPoolParent under persistent ObjectPoolManager.
- UI pool storage is scene-specific because Canvas is scene-specific.
- Do not serialize a Canvas reference directly on the DDOL ObjectPoolManager.
- Scene UI pool root can register its Transform through `SetUIPoolParent()`.

Current interfaces:
```csharp
public interface IPoolable
{
    void SetPool(IPool pool, int objectId);
    void InitPool();
    void InitPoolReturn();
    void ReturnPool();
}

public interface IPool
{
    IPoolable Get(int objectId, Vector3 position, Transform parent = null);
    void Return(IPoolable item, int objectId);
    void Prewarm(int objectId, int poolSize, Transform parent);
}
```

`ReturnPool()` allows a pooled object to request its own return through its assigned pool.

## Factory
Factory responsibilities:
- ID -> prefab mapping.
- Instantiate actual prefab instances.
- ObjectPool controls reuse, queues, pool parents, and lifecycle.
- Do not move pooling responsibility into Factory.

Conceptual flow:
```text
PoolManager -> Pool -> Factory.Create() -> Instantiate -> Pool controls reuse
```

Factory categories are intended to be extensible:
- Enemy
- UI
- Effect
- etc.

Effects should not be hardcoded around AfterImage; more effects may be added later.

## UI
Previous UI architecture includes UIBase, PopupBase, UIManager, UIButtonGroup, UIName, and HUD grouping.
Rules:
- Avoid Find-based UI references.
- Prefer registries, explicit references, or pool/factory registration.
- Do not force every button to inherit UIBase solely to identify a popup.
- Avoid stale UI dictionary entries and duplicate event subscriptions across scene changes.
- Keep popup lifecycle separate from persistent scene HUD lifecycle when appropriate.

## Historical Calendar/Event Architecture
Earlier work included CalendarManager, EventManager, GameDate, GameEventBridge, EventFactory, and ScriptableObject configs.
This is historical context and should not be assumed to be part of the current Parry implementation unless relevant.
Design lessons:
- Separate responsibilities.
- Use bridges/registries/factories when they reduce direct coupling.
- Avoid repeatedly replacing the whole architecture during implementation.

## Git / Codex Workflow
- Project is a Git repository managed with GitHub Desktop.
- Codex should work from the repository root.
- Inspect current Git status and relevant files before modifying code.
- Preserve uncommitted user work.
- Do not reset/revert commits automatically.
- Keep changes focused on the requested task.
- Avoid destructive Git operations unless explicitly requested.

## Coding Style
- Unity/C#.
- Clear class and method names.
- Prefer explicit responsibility boundaries over clever abstractions.
- Use existing architecture before introducing new infrastructure.
- Avoid comments that merely restate obvious code.
- When changing code, make modified sections easy to identify.
- When practical, provide the full updated file rather than a partial replacement.
- Korean communication is preferred.
- Keep explanations concise unless detailed reasoning is requested.
