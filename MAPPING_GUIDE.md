# ConfigLink 映射配置详细指南

本文档详细介绍了 ConfigLink 的映射配置语法和所有内置转换器的使用方法。

## 📋 目录

- [基本映射语法](#基本映射语法)
- [路径表达式](#路径表达式)
- [转换器链](#转换器链)
- [内置转换器详解](#内置转换器详解)
- [完整示例](#完整示例)
- [高级用法](#高级用法)

## 基本映射语法

### 映射规则结构

每个映射规则包含以下属性：

```json
{
  "source": "源字段路径",
  "target": "目标字段名称",
  "conversion": ["转换器1", "转换器2"],
  "conversion_params": {
    "转换器1": "参数1",
    "转换器2": { "参数": "值" }
  }
}
```

### 简单字段映射

```json
{
  "source": "OrderId",
  "target": "id"
}
```

将源对象的 `OrderId` 字段映射到目标对象的 `id` 字段。

## 路径表达式

### 嵌套对象访问

```json
{
  "source": "Customer.Name",
  "target": "customer_name"
}
```

访问嵌套对象的属性，使用 `.` 分隔符。

### 数组元素访问

```json
{
  "source": "OrderItems[0].ProductName",
  "target": "first_product_name"
}
```

访问数组的特定索引元素。

### 根对象引用

```json
{
  "source": "ShippingAddress",
  "target": "$root",
  "conversion": ["map_object"]
}
```

使用 `$root` 作为目标，将映射结果直接合并到根对象中。

## 转换器链

转换器按照在 `conversion` 数组中的顺序依次执行，前一个转换器的输出作为下一个转换器的输入。

```json
{
  "source": "TotalAmount",
  "target": "total_amount",
  "conversion": ["format", "prepend"],
  "conversion_params": {
    "format": "F2",
    "prepend": "$"
  }
}
```

上述配置的执行顺序：
1. `format` 转换器：`59.97` → `"59.97"`
2. `prepend` 转换器：`"59.97"` → `"$59.97"`

## 内置转换器详解

### 1. format - 格式化转换器

用于格式化日期、数字等数据。

#### 数字格式化

```json
{
  "conversion": ["format"],
  "conversion_params": {
    "format": "F2"
  }
}
```

**示例：**
- `123.456` → `"123.46"`
- `1234.56` → `"1234.56"`

#### 货币格式化

```json
{
  "conversion": ["format"],
  "conversion_params": {
    "format": "C"
  }
}
```

#### 日期格式化

```json
{
  "conversion": ["format"],
  "conversion_params": {
    "format": "yyyy-MM-dd"
  }
}
```

**示例：**
- `"2023-01-15T10:30:00"` → `"2023-01-15"`

### 2. prepend - 前缀添加转换器

在值的前面添加指定的前缀。

```json
{
  "conversion": ["prepend"],
  "conversion_params": {
    "prepend": "Hello "
  }
}
```

**示例：**
- `"world"` → `"Hello world"`
- `123` → `"Hello 123"`

### 3. case - 大小写转换器

转换字符串的大小写格式。

#### 转换为大写

```json
{
  "conversion": ["case"],
  "conversion_params": {
    "case": { "case": "upper" }
  }
}
```

**示例：**
- `"hello world"` → `"HELLO WORLD"`

#### 转换为小写

```json
{
  "conversion": ["case"],
  "conversion_params": {
    "case": { "case": "lower" }
  }
}
```

#### 转换为驼峰命名

```json
{
  "conversion": ["case"],
  "conversion_params": {
    "case": { "case": "camel" }
  }
}
```

**示例：**
- `"hello world test"` → `"helloWorldTest"`

#### 转换为帕斯卡命名

```json
{
  "conversion": ["case"],
  "conversion_params": {
    "case": { "case": "pascal" }
  }
}
```

### 4. trim - 空白字符清理转换器

移除字符串的空白字符。

#### 移除两端空白

```json
{
  "conversion": ["trim"],
  "conversion_params": {
    "trim": { "type": "both" }
  }
}
```

**简化写法：**
```json
{
  "conversion": ["trim"],
  "conversion_params": {
    "trim": "both"
  }
}
```

**示例：**
- `"  hello world  "` → `"hello world"`

#### 移除左侧空白

```json
{
  "conversion": ["trim"],
  "conversion_params": {
    "trim": "left"
  }
}
```

#### 移除右侧空白

```json
{
  "conversion": ["trim"],
  "conversion_params": {
    "trim": "right"
  }
}
```

### 5. replace - 字符串替换转换器

替换字符串中的指定内容。

```json
{
  "conversion": ["replace"],
  "conversion_params": {
    "replace": { "from": "world", "to": "universe" }
  }
}
```

**示例：**
- `"hello world"` → `"hello universe"`

### 6. substring - 字符串截取转换器

提取字符串的子串。

```json
{
  "conversion": ["substring"],
  "conversion_params": {
    "substring": { "start": 0, "length": 5 }
  }
}
```

**示例：**
- `"hello world"` → `"hello"`

### 7. join - 数组连接转换器

将数组元素连接为字符串。

#### 使用逗号连接

```json
{
  "conversion": ["join"],
  "conversion_params": {
    "join": ", "
  }
}
```

**示例：**
- `["apple", "banana", "cherry"]` → `"apple, banana, cherry"`

#### 使用自定义分隔符

```json
{
  "conversion": ["join"],
  "conversion_params": {
    "join": " | "
  }
}
```

**示例：**
- `["one", "two", "three"]` → `"one | two | three"`

### 8. to_array - 对象转数组转换器

从对象中提取指定字段组成数组。

```json
{
  "conversion": ["to_array"],
  "conversion_params": {
    "to_array": ["Street", "City", "State", "ZipCode", "Country"]
  }
}
```

**示例：**
```json
// 输入对象
{
  "Street": "123 Main St",
  "City": "Boston",
  "State": "MA",
  "ZipCode": "02108",
  "Country": "USA"
}

// 输出数组
["123 Main St", "Boston", "MA", "02108", "USA"]
```

### 9. map_array - 数组映射转换器

对数组中的每个元素执行映射规则。

```json
{
  "conversion": ["map_array"],
  "conversion_params": {
    "map_array": [
      {
        "source": "ProductId",
        "target": "product_id"
      },
      {
        "source": "ProductName",
        "target": "product_name"
      },
      {
        "source": "Price",
        "target": "price",
        "conversion": ["format", "prepend"],
        "conversion_params": {
          "format": "F2",
          "prepend": "$"
        }
      }
    ]
  }
}
```

**示例：**
```json
// 输入数组
[
  { "ProductId": 1, "ProductName": "Pen", "Price": 19.99 },
  { "ProductId": 2, "ProductName": "Book", "Price": 29.95 }
]

// 输出数组
[
  { "product_id": 1, "product_name": "Pen", "price": "$19.99" },
  { "product_id": 2, "product_name": "Book", "price": "$29.95" }
]
```

### 10. map_object - 对象映射转换器

对对象执行映射规则，通常与 `$root` 目标一起使用。

```json
{
  "source": "ShippingAddress",
  "target": "$root",
  "conversion": ["map_object"],
  "conversion_params": {
    "map_object": [
      {
        "source": "Street",
        "target": "shipping_street"
      },
      {
        "source": "City",
        "target": "shipping_city"
      }
    ]
  }
}
```

**示例：**
```json
// 输入对象
{
  "Street": "123 Main St",
  "City": "Boston"
}

// 输出（合并到根对象）
{
  "shipping_street": "123 Main St",
  "shipping_city": "Boston"
}
```

### 11. number - 数字转换器

将字符串转换为数字类型。

#### 转换为整数

```json
{
  "conversion": ["number"],
  "conversion_params": {
    "number": { "type": "int" }
  }
}
```

**简化写法：**
```json
{
  "conversion": ["number"],
  "conversion_params": {
    "number": "int"
  }
}
```

**示例：**
- `"123"` → `123`

#### 转换为浮点数

```json
{
  "conversion": ["number"],
  "conversion_params": {
    "number": "double"
  }
}
```

### 12. boolean - 布尔值转换器

转换为布尔值或布尔格式字符串。

#### 转换为布尔值

```json
{
  "conversion": ["boolean"],
  "conversion_params": {
    "boolean": { "output": "boolean" }
  }
}
```

**示例：**
- `"yes"` → `true`
- `"no"` → `false`
- `"true"` → `true`
- `"1"` → `true`

#### 转换为 Yes/No 格式

```json
{
  "conversion": ["boolean"],
  "conversion_params": {
    "boolean": { "output": "yesno" }
  }
}
```

**示例：**
- `true` → `"yes"`
- `false` → `"no"`

#### 转换为数字格式

```json
{
  "conversion": ["boolean"],
  "conversion_params": {
    "boolean": { "output": "numeric" }
  }
}
```

**示例：**
- `true` → `1`
- `false` → `0`

### 13. default - 默认值转换器

为空值设置默认值。

#### 当值为 null 时设置默认值

```json
{
  "conversion": ["default"],
  "conversion_params": {
    "default": { "value": "默认文本", "condition": "null" }
  }
}
```

#### 当值为空字符串时设置默认值

```json
{
  "conversion": ["default"],
  "conversion_params": {
    "default": { "value": "默认文本", "condition": "empty" }
  }
}
```

## 完整示例

### 示例配置文件

```json
{
  "mappings": [
    {
      "source": "OrderId",
      "target": "id"
    },
    {
      "source": "Customer.Name",
      "target": "customer_name"
    },
    {
      "source": "OrderDate",
      "target": "order_date",
      "conversion": ["format"],
      "conversion_params": {
        "format": "yyyy-MM-dd"
      }
    },
    {
      "source": "TotalAmount",
      "target": "total_amount",
      "conversion": ["format", "prepend"],
      "conversion_params": {
        "format": "F2",
        "prepend": "$"
      }
    },
    {
      "source": "Items",
      "target": "items",
      "conversion": ["map_array"],
      "conversion_params": {
        "map_array": [
          {
            "source": "ProductId",
            "target": "product_id"
          },
          {
            "source": "ProductName",
            "target": "product_name"
          },
          {
            "source": "Price",
            "target": "price",
            "conversion": ["format", "prepend"],
            "conversion_params": {
              "format": "F2",
              "prepend": "$"
            }
          }
        ]
      }
    },
    {
      "source": "ShippingAddress",
      "target": "$root",
      "conversion": ["map_object"],
      "conversion_params": {
        "map_object": [
          {
            "source": "Street",
            "target": "shipping_street"
          },
          {
            "source": "City",
            "target": "shipping_city"
          }
        ]
      }
    },
    {
      "source": "BillingAddress",
      "target": "billing_address",
      "conversion": ["to_array", "join"],
      "conversion_params": {
        "to_array": ["Street", "City", "State", "ZipCode", "Country"],
        "join": ", "
      }
    }
  ]
}
```

### 示例源数据

```json
{
  "OrderId": 1001,
  "Customer": {
    "Name": "John Doe"
  },
  "OrderDate": "2025/10/29T15:30:00",
  "TotalAmount": 59.97,
  "Items": [
    {
      "ProductId": 1,
      "ProductName": "Pen",
      "Price": 19.99
    }
  ],
  "ShippingAddress": {
    "Street": "123 Main St",
    "City": "Boston"
  },
  "BillingAddress": {
    "Street": "456 Oak Ave",
    "City": "Springfield",
    "State": "MA",
    "ZipCode": "01234",
    "Country": "USA"
  }
}
```

### 转换结果

```json
{
  "id": 1001,
  "customer_name": "John Doe",
  "order_date": "2025-10-29",
  "total_amount": "$59.97",
  "items": [
    {
      "product_id": 1,
      "product_name": "Pen",
      "price": "$19.99"
    }
  ],
  "shipping_street": "123 Main St",
  "shipping_city": "Boston",
  "billing_address": "456 Oak Ave, Springfield, MA, 01234, USA"
}
```

## 高级用法

### 1. 复杂转换器链

```json
{
  "source": "Description",
  "target": "formatted_description",
  "conversion": ["trim", "case", "prepend"],
  "conversion_params": {
    "trim": "both",
    "case": { "case": "upper" },
    "prepend": "PRODUCT: "
  }
}
```

### 2. 条件默认值

```json
{
  "source": "OptionalField",
  "target": "required_field",
  "conversion": ["default", "trim"],
  "conversion_params": {
    "default": { "value": "N/A", "condition": "null" },
    "trim": "both"
  }
}
```

### 3. 数据类型转换

```json
{
  "source": "StringNumber",
  "target": "numeric_value",
  "conversion": ["number"],
  "conversion_params": {
    "number": "double"
  }
}
```

### 4. 嵌套数组处理

```json
{
  "source": "Categories",
  "target": "category_list",
  "conversion": ["map_array", "join"],
  "conversion_params": {
    "map_array": [
      {
        "source": "Name",
        "target": "name"
      }
    ],
    "join": ", "
  }
}
```

## 💡 最佳实践

1. **转换器顺序很重要**：确保转换器的执行顺序符合预期
2. **参数命名匹配**：`conversion_params` 中的键名必须与转换器名称完全匹配
3. **类型兼容性**：确保前一个转换器的输出类型与下一个转换器的输入类型兼容
4. **错误处理**：无效的转换会返回原值或 null，注意处理这些情况
5. **性能考虑**：避免过长的转换器链，合理使用缓存

## 🔧 自定义转换器

如果内置转换器不满足需求，可以实现 `IConverter` 接口创建自定义转换器：

```csharp
public class CustomConverter : IConverter
{
    public object? Convert(object? value, MappingRule rule, MappingEngine? engine)
    {
        // 自定义转换逻辑
        return transformedValue;
    }
}
```

然后在映射引擎中注册：

```csharp
var engine = new MappingEngine(rules);
engine.RegisterConverter("custom", new CustomConverter());
```

---

这份文档涵盖了 ConfigLink 映射配置的所有功能和用法。如需更多信息，请参考项目源代码和测试用例。