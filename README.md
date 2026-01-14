# Rapki Skill Editor for Unity

노드 기반 비주얼 스킬 에디터. 기획자가 프로그래머 없이 스킬 효과를 조합하고 테스트할 수 있습니다.

## 특징

- **비주얼 노드 에디터** - 드래그 & 드롭으로 스킬 로직 구성
- **모듈화된 스킬** - 스킬 효과를 독립적인 모듈로 분리
- **런타임 실행** - 에디터에서 만든 스킬을 게임에서 바로 실행
- **독립적** - 외부 의존성 없이 어떤 프로젝트에도 적용 가능
- **Attribute 자동 등록** - 클래스에 Attribute만 붙이면 자동 등록

---

## 설치

`Core` 폴더를 프로젝트에 복사

```
Assets/
└── SkillEditor/
    ├── Core/                    # 필수
    │   ├── ISkillBehaviour.cs
    │   ├── ISkillContext.cs
    │   ├── SkillBehaviourRegistry.cs
    │   ├── SkillBehaviourBase.cs
    │   ├── SkillEditorConfig.cs
    │   ├── CompiledData.cs
    │   ├── NodeData.cs
    │   ├── SkillGraphData.cs
    │   ├── SkillGraphCompiler.cs
    │   └── PipelineExecutor.cs
    └── Examples/                # 선택 (참고용)
        ├── DamageSkill.cs
        ├── DefenceSkill.cs
        └── HealSkill.cs
```

---

## 빠른 시작

### 1. 스킬 만들기

```csharp
using SkillEditor.Core;
using UnityEngine;

[SkillBehaviour("화염구", "#FF5500", "대상에게 불 피해를 준다")]
public class FireballSkill : SkillBehaviourBase
{
    public override string Name => "화염구";
    public override string Description => "대상에게 불 피해를 준다";
    public override Color NodeColor => new Color(1f, 0.33f, 0f);
    
    public override void Execute(ISkillContext context, int value)
    {
        // 게임의 Entity 타입으로 캐스팅
        var target = context.GetTarget<Monster>();
        target.TakeDamage(value);
    }
}
```

### 2. 런타임에서 실행

```csharp
using SkillEditor.Core;

public class SkillSystem : MonoBehaviour
{
    public SkillGraphData skillData;
    
    public void UseSkill(object self, object target)
    {
        var context = new SkillContext(self, target);
        skillData.Execute(0, context);  // pipeline 0 실행
    }
}
```

---

## Core API

### ISkillBehaviour
```csharp
public interface ISkillBehaviour
{
    string Name { get; }
    string Description { get; }
    Color NodeColor { get; }
    void Execute(ISkillContext context, int value);
}
```

### ISkillContext
```csharp
public interface ISkillContext
{
    object Self { get; }
    object Target { get; }
    T GetSelf<T>() where T : class;
    T GetTarget<T>() where T : class;
}
```

### SkillBehaviourRegistry
```csharp
// 자동 등록된 스킬 사용
var skill = SkillBehaviourRegistry.Instance.Get("화염구");

// 수동 등록
SkillBehaviourRegistry.Instance.Register(new MySkill());

// 새로고침
SkillBehaviourRegistry.Instance.Refresh();
```

### PipelineExecutor
```csharp
PipelineExecutor.Execute(
    pipeline,                              // CompiledPipeline
    context,                               // ISkillContext
    SkillBehaviourRegistry.Instance        // ISkillBehaviourRegistry
);
```

### SkillGraphCompiler
```csharp
// 기본 설정으로 컴파일
SkillGraphCompiler.Compile(skillData);

// 커스텀 설정으로 컴파일
SkillGraphCompiler.Compile(skillData, myConfig);
```

---

## 설정 커스터마이징

`Assets > Create > SkillEditor > Config` 로 설정 파일 생성

```csharp
// 기본값
SelfPortName = "시전자"
TargetPortName = "대상"
ConditionPortName = "조건"
ContextSelfPortName = "Self"
ContextTargetPortName = "Target"
MaxPipelines = 3
PipelineSlotFormat = "슬롯 {0}"
```

---

## 스킬 노드 구현 예시

```csharp
[SkillBehaviour("독", "#9B59B6")]
public class PoisonSkill : SkillBehaviourBase
{
    public override string Name => "독";
    public override void Execute(ISkillContext context, int value)
    {
        var target = context.GetTarget<Character>();
        target.AddDebuff(new Poison(value, duration: 3));
    }
}
```
```csharp
[SkillBehaviour("드로우", "#3498DB")]
public class DrawSkill : SkillBehaviourBase
{
    public override string Name => "드로우";
    public override void Execute(ISkillContext context, int value)
    {
        var player = context.GetSelf<Player>();
        player.DrawCards(value);
    }
}
```
```csharp
[SkillBehaviour("범위공격", "#E74C3C")]
public class AoeSkill : SkillBehaviourBase
{
    public override string Name => "범위공격";
    public override void Execute(ISkillContext context, int value)
    {
        var tower = context.GetSelf<Tower>();
        foreach (var enemy in tower.GetEnemiesInRange())
            enemy.TakeDamage(value);
    }
}
```

---

## 라이선스

MIT License
