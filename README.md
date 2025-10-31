# ConfigLink

一个强大的 .NET 配置映射和数据转换库，支持 JSON 配置驱动的数据映射、多种数据转换器以及多平台 API 集成。

## 🌟 特性

- **配置驱动的数据映射**: 通过 JSON 配置文件定义复杂的数据映射规则
- **丰富的数据转换器**: 内置 13 种常用的数据转换器
- **API 集成管理**: 支持多平台 API 配置和调用管理
- **灵活的认证方式**: 支持 Basic、Bearer Token、API Key 等多种认证方式
- **场景化配置**: 支持基于场景的 API 调用配置
- **高度可扩展**: 支持自定义转换器和认证处理器

## 🚀 快速开始

### 安装

```bash
dotnet add package ConfigLink
```

### 基本用法

#### 1. 定义映射规则

创建 `mapping.json` 配置文件：

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
    }
  ]
}
```

#### 2. 使用映射引擎

```csharp
using ConfigLink;
using System.Text.Json;

// 解析映射配置
var mappingJson = File.ReadAllText("mapping.json");
var doc = JsonDocument.Parse(mappingJson);
var rules = doc.RootElement
    .GetProperty("mappings")
    .Deserialize<List<MappingRule>>();

// 创建映射引擎
var engine = new MappingEngine(rules);

// 源数据
var sourceData = new
{
    OrderId = 1001,
    Customer = new { Name = "John Doe" },
    OrderDate = "2025/10/29",
    TotalAmount = 59.97
};

// 执行映射转换
var result = engine.Transform(sourceData);

// 结果
// {
//   "id": 1001,
//   "customer_name": "John Doe",
//   "order_date": "2025-10-29",
//   "total_amount": "$59.97"
// }
```

## 📚 数据转换器

ConfigLink 提供了丰富的内置转换器：

| 转换器 | 功能 | 示例 |
|--------|------|------|
| `format` | 格式化数据（日期、数字等） | `"2025/10/29" → "2025-10-29"` |
| `prepend` | 在值前添加前缀 | `"59.97" → "$59.97"` |
| `case` | 大小写转换 | `"Hello" → "hello"` |
| `trim` | 去除空白字符 | `" text " → "text"` |
| `replace` | 字符串替换 | `"hello world" → "hello universe"` |
| `substring` | 字符串截取 | `"hello" → "hel"` |
| `join` | 数组连接为字符串 | `["a", "b"] → "a,b"` |
| `to_array` | 转换为数组 | `"a,b,c" → ["a", "b", "c"]` |
| `map_array` | 数组元素映射 | 对数组中每个元素执行映射 |
| `map_object` | 对象映射 | 对嵌套对象执行映射 |
| `number` | 数字转换 | `"123" → 123` |
| `boolean` | 布尔值转换 | `"true" → true` |
| `default` | 设置默认值 | `null → "默认值"` |

### 转换器链式调用

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

## 🌐 API 集成

### API 配置

创建 `api.config.json`：

```json
{
  "PlatformA": {
    "endpoint": "https://api.a.com/v1/users",
    "auth": "basic",
    "username": "admin",
    "password": "secret",
    "headers": {
      "Content-Type": "application/json"
    },
    "timeoutSeconds": 30,
    "retry": 3
  },
  "PlatformB": {
    "endpoint": "https://api.b.com/webhook",
    "auth": "bearer",
    "token": "your-bearer-token",
    "headers": {
      "Content-Type": "application/json"
    }
  }
}
```

### 场景配置

创建 `scenario.json`：

```json
{
  "subscribe": {
    "PlatformA": {
      "path": "/api/v1/subscribe",
      "method": "POST",
      "mappings": [
        {
          "source": "email",
          "target": "emailAddress"
        },
        {
          "source": "firstName",
          "target": "firstName"
        }
      ]
    }
  }
}
```

### 使用 API 管理器

```csharp
using ConfigLink;

// 加载配置
var apiConfig = ApiConfigs.FromFile("api.config.json");
var scenarioConfig = ScenarioConfigs.FromFile("scenario.json");

// 创建 API 管理器
using var apiManager = new ApiManager(apiConfig, scenarioConfig);

// 执行场景调用
var userData = new { email = "user@example.com", firstName = "John" };
var result = await apiManager.ExecuteScenarioAsync("subscribe", "PlatformA", userData);
```

## 🔧 高级特性

### 复杂数据映射

支持嵌套对象和数组的复杂映射：

```json
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
      }
    ]
  }
}
```

### 路径表达式

支持复杂的 JSON 路径表达式：

- `Customer.Name` - 嵌套对象属性
- `Items[0].ProductName` - 数组元素访问
- `$root.ShippingAddress` - 根对象引用

### 自定义转换器

实现 `IConverter` 接口创建自定义转换器：

```csharp
public class CustomConverter : IConverter
{
    public object? Convert(object? value, Dictionary<string, object>? parameters)
    {
        // 自定义转换逻辑
        return transformedValue;
    }
}

// 注册自定义转换器
engine.RegisterConverter("custom", new CustomConverter());
```

## 🏗️ 项目结构

```
ConfigLink/
├── ConfigLink/                    # 核心库
│   ├── Api/                      # API 管理功能
│   ├── Converters/               # 数据转换器
│   ├── IConverter.cs             # 转换器接口
│   ├── MappingEngine.cs          # 映射引擎
│   └── MappingRule.cs            # 映射规则模型
├── Test/                         # 测试项目
│   ├── config/                   # 测试配置文件
│   ├── Api/                      # API 测试
│   └── Converters/               # 转换器测试
└── TestConsole/                  # 控制台测试项目
```

## 🧪 运行测试

```bash
# 运行所有测试
dotnet test

# 运行特定测试
dotnet test --filter "TestClassName"

# 生成测试覆盖率报告
dotnet test --collect:"XPlat Code Coverage"
```

## 📦 构建

```bash
# 构建项目
dotnet build

# 发布 Release 版本
dotnet build -c Release

# 打包 NuGet 包
dotnet pack -c Release
```

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 提交 Pull Request

## 👥 维护者

- [@SeriaWei](https://github.com/SeriaWei)

---

如果这个项目对你有帮助，请给它一个 ⭐️！