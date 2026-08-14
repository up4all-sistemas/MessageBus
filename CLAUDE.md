# CLAUDE.md — Diretrizes de Desenvolvimento do Up4All.MessageBus

Este arquivo serve como guia rápido para compilação, teste, formatação e padrões de codificação para o projeto **Up4All.MessageBus**.

## 🛠️ Comandos de Build e Execução

### Restaurar Dependências
```bash
dotnet restore Up4All.MessageBus.sln
```

### Compilar a Solução
```bash
dotnet build Up4All.MessageBus.sln -c Debug
dotnet build Up4All.MessageBus.sln -c Release
```

### Limpar Artefatos de Build
```bash
dotnet clean Up4All.MessageBus.sln
```

---

## 🧪 Comandos de Testes

Atualmente, o projeto possui uma estrutura de testes em `Up4All.Framework.MessageBus.Tests` baseada em **NUnit 4** e um projeto de mocks auxiliares em `tests/Up4All.Framework.MessageBus.Mocks`.

### Executar Todos os Testes
```bash
dotnet test Up4All.MessageBus.sln
```

### Executar Testes com Coleta de Cobertura (Coverlet)
```bash
dotnet test Up4All.MessageBus.sln --collect:"XPlat Code Coverage"
```

---

## 📝 Padrões de Código e Diretrizes Estilísticas

### Convenções de Nomenclatura
- **Namespaces**: Seguem a estrutura física das pastas (ex: `Up4All.Framework.MessageBus.Abstractions.Interfaces`).
- **Interfaces**: Devem começar com o prefixo `I` (ex: `IMessageBusPublisherAsync`).
- **Classes**: PascalCase (ex: `RabbitMQQueueAsyncClient`).
- **Módulos Assíncronos**: Métodos que retornam `Task` ou `ValueTask` devem obrigatoriamente terminar com o sufixo `Async` (ex: `SendAsync`, `RegisterHandlerAsync`).
- **Campos Privados**: Devem usar o prefixo `_` (underscore) e camelCase (ex: `private readonly IMessageBusPublisherAsync _publisher`).

### Funcionalidades do C# e .NET
- **Versão do C#**: C# 12 (especificado como `<LangVersion>latest</LangVersion>` em `Directory.Build.props`).
- **Primary Constructors**: Preferir o uso de construtores primários para injeção de dependência em classes de serviços (ex: `public class ConsumerHandler<...>(...)`).
- **File-Scoped Namespaces**: Utilizar namespaces no escopo do arquivo para reduzir o aninhamento de chaves `{}`.
- **Nullable Reference Types**: Ativar `<Nullable>enable</Nullable>` nos novos projetos ou ao refatorar existentes para segurança de tipos nulos.

### Padrões Arquiteturais
- **Abstração (Abstractions)**: Todo o comportamento essencial do Message Bus (publishers, consumers, pipelines, options, messages) está contido no projeto `Abstractions`. **Nunca** acople lógica específica de um broker nas interfaces globais.
- **Provedores de Infraestrutura**: Cada broker (RabbitMQ, Azure Service Bus, Kafka) possui sua própria implementação isolada em projetos dedicados que herdam e implementam as abstrações de `Abstractions`.
- **Transferência entre Brokers (TransferHelper)**: Facilita o encaminhamento de mensagens (Shovel/Forward) entre diferentes brokers utilizando pipelines e transformações.
- **Injeção de Dependência**: Os provedores de infraestrutura devem fornecer métodos de extensão `IoCExtensions` intuitivos sob `Up4All.Framework.MessageBus.<Broker>.Extensions` para fácil registro no `IServiceCollection`.
- **Tratamento de Resiliência**: Utilização de **Polly** nos provedores de infraestrutura para gerenciar tentativas de conexão e resiliência a falhas transitórias.
- **Observabilidade**: Integração nativa com **OpenTelemetry** no pipeline de mensagens (`OpenTelemetryExtensions`) para propagação de contexto distribuído e tracing automatizado.
