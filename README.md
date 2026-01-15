# Rapki Skill Editor for Unity

노드 기반 비주얼 스킬 에디터. 기획자가 프로그래머 없이 스킬 효과를 조합하고 테스트할 수 있습니다.

## 특징

- **비주얼 노드 에디터** - 드래그 & 드롭으로 스킬 로직 구성
- **모듈화된 스킬** - 스킬 효과를 독립적인 모듈로 분리
- **조건 분기** - Branch, Compare, AND/OR/NOT 노드로 조건부 실행
- **동적 값 계산** - 엔티티 속성, 사칙연산, 커스텀 프로세서 지원
- **런타임 실행** - 에디터에서 만든 스킬을 게임에서 바로 실행
- **독립적** - 외부 의존성 없이 어떤 프로젝트에도 적용 가능
- **Attribute 자동 등록** - 클래스에 Attribute만 붙이면 자동 등록

## 설치

Core 폴더를 프로젝트에 복사
```
Assets/
└── SkillEditor/
    ├── Core/                    # 필수 (런타임)
    ├── Editor/                  # 필수 (에디터)
    └── Examples/                # 선택 (참고용)
```

## 빠른 시작

### 1. 스킬 효과 만들기
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
        var target = context.GetTarget<Monster>();
        target.TakeDamage(value);
    }
}
```

### 2. 엔티티 속성 정의하기
```csharp
using SkillEditor.Core;

[PropertyAccessor("HP", "Status")]
public class HPAccessor : IPropertyAccessor
{
    public string Name => "HP";
    public string Category => "Status";
    
    public float GetValue(object entity)
    {
        return ((Character)entity).CurrentHP;
    }
    
    public void SetValue(object entity, float value)
    {
        ((Character)entity).CurrentHP = (int)value;
    }
}
```

### 3. 커스텀 값 프로세서 만들기
```csharp
using SkillEditor.Core;
using UnityEngine;

[ValueProcessor("Percent", "#CC8888", 2, "Value", "Percent")]
public class PercentProcessor : IValueProcessor
{
    public string Name => "Percent";
    public Color NodeColor => new Color(0.8f, 0.5f, 0.5f);
    public int InputCount => 2;
    
    public float Process(params float[] inputs)
    {
        return inputs[0] * inputs[1] / 100f;
    }
}
```

### 4. 런타임에서 실행
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

## 실전 사용 예시

### 카드 게임
```csharp
public class CardBattle : MonoBehaviour
{
    public void PlayCard(Card card, Player player, Enemy enemy)
    {
        var context = new SkillContext(player, enemy);
        card.skillData.Execute(0, context);
    }
}

// 스킬 효과들
[SkillBehaviour("공격", "#E74C3C")]
public class AttackSkill : SkillBehaviourBase
{
    public override string Name => "공격";
    public override void Execute(ISkillContext context, int value)
    {
        var target = context.GetTarget<Enemy>();
        target.TakeDamage(value);
    }
}

[SkillBehaviour("방어", "#3498DB")]
public class DefendSkill : SkillBehaviourBase
{
    public override string Name => "방어";
    public override void Execute(ISkillContext context, int value)
    {
        var self = context.GetSelf<Player>();
        self.AddBlock(value);
    }
}

[SkillBehaviour("드로우", "#2ECC71")]
public class DrawSkill : SkillBehaviourBase
{
    public override string Name => "드로우";
    public override void Execute(ISkillContext context, int value)
    {
        var self = context.GetSelf<Player>();
        self.DrawCards(value);
    }
}
```

### 타워 디펜스
```csharp
public class Tower : MonoBehaviour
{
    public SkillGraphData attackSkill;
    public float attackInterval = 1f;
    
    private void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            var target = FindNearestEnemy();
            if (target != null)
            {
                var context = new SkillContext(this, target);
                attackSkill.Execute(0, context);
                nextAttackTime = Time.time + attackInterval;
            }
        }
    }
}

[SkillBehaviour("슬로우", "#9B59B6")]
public class SlowSkill : SkillBehaviourBase
{
    public override string Name => "슬로우";
    public override void Execute(ISkillContext context, int value)
    {
        var target = context.GetTarget<Enemy>();
        target.ApplyDebuff(new SlowDebuff(value / 100f, duration: 2f));
    }
}

[SkillBehaviour("스플래시", "#E67E22")]
public class SplashSkill : SkillBehaviourBase
{
    public override string Name => "스플래시";
    public override void Execute(ISkillContext context, int value)
    {
        var tower = context.GetSelf<Tower>();
        var mainTarget = context.GetTarget<Enemy>();
        
        foreach (var enemy in tower.GetEnemiesInRange(mainTarget.position, 3f))
        {
            enemy.TakeDamage(value);
        }
    }
}
```

### RPG 전투
```csharp
public class BattleSystem : MonoBehaviour
{
    public void UseSkill(Character caster, Character target, SkillGraphData skill)
    {
        var context = new SkillContext(caster, target);
        
        // 파이프라인 0: 메인 효과
        skill.Execute(0, context);
        
        // 파이프라인 1: 추가 효과 (있으면)
        if (skill.compiledPipelines.Count > 1)
            skill.Execute(1, context);
    }
}

[SkillBehaviour("흡혈", "#8E44AD")]
public class DrainSkill : SkillBehaviourBase
{
    public override string Name => "흡혈";
    public override void Execute(ISkillContext context, int value)
    {
        var self = context.GetSelf<Character>();
        var target = context.GetTarget<Character>();
        
        int damage = target.TakeDamage(value);
        self.Heal(damage / 2);
    }
}

[SkillBehaviour("버프", "#F1C40F")]
public class BuffSkill : SkillBehaviourBase
{
    public override string Name => "버프";
    public override void Execute(ISkillContext context, int value)
    {
        var self = context.GetSelf<Character>();
        self.AddBuff(new AttackBuff(value, duration: 3));
    }
}

// 엔티티 속성
[PropertyAccessor("HP", "Status")]
public class HPAccessor : IPropertyAccessor
{
    public string Name => "HP";
    public string Category => "Status";
    public float GetValue(object entity) => ((Character)entity).HP;
    public void SetValue(object entity, float value) => ((Character)entity).HP = (int)value;
}

[PropertyAccessor("MaxHP", "Status")]
public class MaxHPAccessor : IPropertyAccessor
{
    public string Name => "MaxHP";
    public string Category => "Status";
    public float GetValue(object entity) => ((Character)entity).MaxHP;
    public void SetValue(object entity, float value) { }
}

[PropertyAccessor("Attack", "Combat")]
public class AttackAccessor : IPropertyAccessor
{
    public string Name => "Attack";
    public string Category => "Combat";
    public float GetValue(object entity) => ((Character)entity).Attack;
    public void SetValue(object entity, float value) => ((Character)entity).Attack = (int)value;
}
```

### 멀티 타겟 스킬
```csharp
public class MultiTargetSkill : MonoBehaviour
{
    public SkillGraphData aoeSkill;
    
    public void CastAOE(Character caster, List<Character> targets)
    {
        foreach (var target in targets)
        {
            var context = new SkillContext(caster, target);
            aoeSkill.Execute(0, context);
        }
    }
}
```

## 노드 종류

### 흐름 노드
- **Context** - Self/Target 엔티티 제공
- **SkillBehaviour** - 실제 스킬 효과 실행
- **Branch** - 조건에 따라 분기

### 값 노드
- **Number** - 상수 값
- **Get Property** - 엔티티 속성 읽기 (HP, Attack 등)
- **Math** - 사칙연산 (+, -, ×, ÷)
- **Processor** - 커스텀 값 가공

### 조건 노드
- **Compare** - 값 비교 (==, >, <, >=, <=)
- **AND / OR / NOT** - 논리 연산

### 유틸 노드
- **Comment** - 메모용 주석

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

### IPropertyAccessor
```csharp
public interface IPropertyAccessor
{
    string Name { get; }
    string Category { get; }
    float GetValue(object entity);
    void SetValue(object entity, float value);
}
```

### IValueProcessor
```csharp
public interface IValueProcessor
{
    string Name { get; }
    Color NodeColor { get; }
    int InputCount { get; }
    float Process(params float[] inputs);
}
```

## Registry 사용법

### SkillBehaviourRegistry
```csharp
// 자동 등록된 스킬 가져오기
var skill = SkillBehaviourRegistry.Instance.Get("화염구");

// 모든 스킬 정보
foreach (var info in SkillBehaviourRegistry.Instance.Infos)
{
    Debug.Log($"{info.Key}: {info.Value.Color}");
}

// 새로고침 (어셈블리 변경 후)
SkillBehaviourRegistry.Instance.Refresh();
```

### PropertyAccessorRegistry
```csharp
// 속성 값 가져오기
var hp = PropertyAccessorRegistry.Instance.GetValue("HP", entity);

// 카테고리별 속성
var statusProps = PropertyAccessorRegistry.Instance.ByCategory["Status"];
```

### ValueProcessorRegistry
```csharp
// 프로세서 가져오기
var processor = ValueProcessorRegistry.Instance.Get("Percent");
var result = processor.Process(100f, 30f);  // 30
```

## 설정 커스터마이징

`Assets > Create > SkillEditor > Config`로 설정 파일 생성
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

## 그래프 예시

### 기본 데미지
```
[Context] ─Self→ [데미지 (10)]
          └Target→
```

### 조건부 회복
```
[Context] ─Self→ [Get: Target.HP] → [Compare: < 50] → [Branch] → [회복]
          └Target→                   ↑                          
                    [Number: 50] ────┘
```
"Target HP가 50 미만이면 회복"

### 퍼센트 데미지
```
[Context] ─Target→ [Get: Target.MaxHP] → [Percent] → [데미지]
                   [Number: 30] ────────↗
```
"Target 최대체력의 30% 데미지"

### 공격력 기반 데미지
```
[Context] ─Self→ [Get: Self.Attack] → [Math (+)] → [데미지]
                 [Number: 5] ────────↗
```
"Self 공격력 + 5 데미지"

## 라이선스

MIT License
