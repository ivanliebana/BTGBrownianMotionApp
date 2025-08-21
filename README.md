# BTG.BrownianMotionApp — Simulador de Movimento Browniano (.NET MAUI • .NET 9)

Aplicação desktop em **.NET MAUI** (Windows/macOS) para simular **caminhos de preço** via Movimento Browniano Geométrico (GBM) simplificado, com **múltiplas simulações**, **gráfico customizável** (cores, estilo e espessura de linha), **eixos com escala e rótulos**, **entradas enriquecidas** (Slider/Stepper) e **responsividade**.

> **Stack**: .NET 9, .NET MAUI, WinUI 3 (Windows).  
> **Padrão**: MVVM + DI, desenho custom com `GraphicsView` (`IDrawable`).

---

## Sumário
- [Requisitos](#requisitos)
- [Como rodar](#como-rodar)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Arquitetura e componentes](#arquitetura-e-componentes)
  - [Serviço de simulação](#serviço-de-simulação)
  - [ViewModel](#viewmodel)
  - [ChartDrawable (gráfico)](#chartdrawable-gráfico)
  - [Página XAML](#página-xaml)
  - [Converters](#converters)
  - [Behavior de entrada numérica](#behavior-de-entrada-numérica)
- [Personalização do gráfico](#personalização-do-gráfico)
- [Responsividade](#responsividade)
- [Testes (xUnit)](#testes-xunit)
- [Publicação](#publicação)
- [Roadmap](#roadmap)

---

## Requisitos

- **SDK .NET 9** instalado.
- **Workload .NET MAUI**:
  ```bash
  dotnet workload install maui
  ```
- **Windows 10 2004 (build 19041)+** para WinUI 3.
- **IDE**: Visual Studio (com .NET MAUI) **ou** VS Code + CLI.

---

## Como rodar

### Via CLI (Windows)
```bash
dotnet restore
dotnet build -c Debug
dotnet build -t:Run -f net9.0-windows10.0.19041.0
```

### Via Visual Studio
1. Selecione o projeto **.NET MAUI**.
2. Escolha o alvo **Windows**.
3. Pressione **F5** (depurar) ou **Ctrl+F5** (executar).

> **DI obrigatório** (em `MauiProgram.cs`):
```csharp
builder.Services.AddSingleton<BTG.ClientsApp.Services.IBrownianService, BTG.ClientsApp.Services.BrownianService>();
builder.Services.AddSingleton<BTG.ClientsApp.ViewModels.BrownianViewModel>();
builder.Services.AddSingleton<BTG.ClientsApp.Views.BrownianPage>();
```

---

## Estrutura do projeto

```
BTG.BrownianMotionApp/
  ├─ Views/
  │   ├─ BrownianPage.xaml
  │   └─ BrownianPage.xaml.cs
  ├─ ViewModels/
  │   └─ BrownianViewModel.cs
  ├─ Services/
  │   ├─ IBrownianService.cs
  │   └─ BrownianService.cs
  ├─ Graphics/
  │   └─ ChartDrawable.cs
  ├─ Converters/
  │   ├─ PercentDisplayConverter.cs
  │   └─ LessThanConverter.cs         (opcional)
  ├─ Behaviors/
  │   └─ IntegerOrCommaDecimalBehavior.cs
  ├─ Core/Input/
  │   ├─ NumberInputSettings.cs
  │   └─ NumberInputProcessor.cs
  └─ MauiProgram.cs
```

---

## Arquitetura e componentes

- **MVVM**: `BrownianViewModel` mantém estado/comandos; a View (XAML) só faz binding.
- **Injeção de Dependência**: registro em `MauiProgram.cs` para Service/VM/Page.
- **Desenho custom**: `ChartDrawable` (implementa `IDrawable`) renderiza linhas, eixos, rótulos e grid no `GraphicsView`.
- **Converters/Behaviors**: UX amigável p/ percentual e números com vírgula.

### Serviço de simulação
**Assinaturas** (formato do anexo):
```csharp
double[]  GenerateBrownianMotion(double sigma, double mean, double initialPrice, int numDays, int? seed = null);
double[][] GenerateMultiple(double sigma, double mean, double initialPrice, int numDays, int simulations, int? seed = null);
```
**Modelo diário**:  
`r = mean + sigma * Z` (Z ~ N(0,1))  
`S[t] = S[t-1] * exp(r)`  
> `mean` e `sigma` são **diários** (não anualizados).

Exemplo:
```csharp
var svc = new BrownianService();
var sims = svc.GenerateMultiple(0.20, 0.01, 100, 252, 5, seed: 42);
```

### ViewModel
- Parâmetros: `InitialPrice`, `Volatility`, `MeanReturn`, `Steps`, `Simulations`, `ShowAxes`.
- Personalização: `LineThickness`, `SelectedLineStyle` (Sólida/Tracejada/Pontilhada), `SelectedColorName`.
- Comandos: `RunCommand` (gera séries), `ClearCommand` (limpa).
- Evento: `RequestRedraw` (a View chama `GraphicsView.Invalidate()`).
- Exposição do desenho: `ChartDrawable`.

### ChartDrawable (gráfico)
- Renderiza **múltiplas séries** com paleta derivada da cor base.
- **Eixos com grid e rótulos**: X = “Steps”; Y = “Preço” desenhado **na vertical à esquerda** (evita colisão).
- Propriedades:
  - `StrokeSize` (espessura), `LineStyle` (Sólida/Tracejada/Pontilhada), `BaseColor`, `ShowAxes`.
- Métodos:
  - `SetSeries(IEnumerable<double[]>)`, `Clear()`, `IsEmpty`.

**Ligação do gráfico** (code-behind da página):
```csharp
ChartView.Drawable = vm.ChartDrawable;
vm.RequestRedraw += (_, __) => ChartView.Invalidate();
```

### Página XAML
- Tema escuro, **gráfico em card** à esquerda, **painel** à direita com:
  - Entradas (`Entry`) + **Sliders** e **Steppers**,
  - **Picker** de cor base e estilo de linha,
  - **Switch** “Mostrar eixos”,
  - Botões **Gerar simulação** e **Limpar**.
- **Responsividade** no `code-behind` (veja seção abaixo).

### Converters
- **`PercentDisplayConverter`**: UI exibe “20” ↔ VM recebe `0.20`.
- **`LessThanConverter`** (opcional): auxilia triggers de largura (se usar apenas XAML).

### Behavior de entrada numérica
- **`IntegerOrCommaDecimalBehavior`** (UI) delega ao núcleo **`NumberInputProcessor`** (puro/testável):
  - Inteiros ou decimais com vírgula (ex.: `27,8`), converte ponto→vírgula,
  - Limites de dígitos opcionais, negativo opcional,
  - Suporta edição parcial (ex.: `27,`).

Uso:
```xml
<Entry Text="{Binding AlgumValor}">
  <Entry.Behaviors>
    <behaviors:IntegerOrCommaDecimalBehavior MaxFractionDigits="1" />
  </Entry.Behaviors>
</Entry>
```

---

## Personalização do gráfico

- **Cor base**: Picker → `SelectedColorName` → `ChartDrawable.BaseColor`.
- **Espessura**: Slider → `LineThickness` (clamp 1–10) → `StrokeSize`.
- **Estilo**: Picker → `SelectedLineStyle` (“Sólida”/“Tracejada”/“Pontilhada”) → `LineStyle`.
- **Eixos**: Switch → `ShowAxes` → `ChartDrawable.ShowAxes`.

Cada alteração notifica `RequestRedraw` para repintar o `GraphicsView`.

---

## Responsividade

Implementada em `OnSizeAllocated` (code-behind), movendo painel/gráfico com `Grid.SetRow/SetColumn/SetColumnSpan` conforme a largura:

- **≥ 900 px**: duas colunas (gráfico | painel).
- **< 900 px**: painel desce; gráfico ocupa 2 colunas.

---

## Testes (xUnit)

### Execução
```bash
dotnet test .\BTG.ClientsApp.Tests```

### Abrangência
- **Serviço (`BrownianService`)**  
  Comprimento/positividade; determinismo por seed; seeds diferentes; exceções para parâmetros inválidos.
- **ViewModel**  
  `Run/Clear` populam/limpam séries e disparam `RequestRedraw`; personalização propaga a `ChartDrawable` (eixos, estilo, espessura, cor); clamp de espessura.
- **Converters**  
  `PercentDisplayConverter`: ida/volta com “27,8”, “27.8”, “15,5 %”, inválidos → `0.0`.
- **Behavior (núcleo)**  
  `NumberInputProcessor`: aceita inteiro/decimal com vírgula; converte ponto; limites de dígitos; negativo opcional; finalização remove vírgula pendurada e normaliza.

> Observação: testes que instanciam controles MAUI (ex.: `Entry`) exigem Dispatcher do WinUI; por isso o Behavior é testado via **núcleo puro**, evitando `COMException` no runner.

---

## Publicação

### Windows (executável “self-contained” simples)
```bash
dotnet publish -f net9.0-windows10.0.19041.0 -c Release -p:WindowsPackageType=None
```
Saída típica: `bin/Release/net9.0-windows10.0.19041.0/win10-x64/publish/`

> Para MSIX/assinatura, use o Visual Studio (Propriedades do projeto) ou parâmetros adicionais de `publish`.

---

## Roadmap

- Exportar gráfico (PNG/SVG) e dados (CSV).
- Intervalos de confiança / percentis como envelope visual.
- Temas (claro/escuro) com `AppThemeBinding`.
- Zoom/pan no gráfico (gestos no `GraphicsView`).
- Presets de parâmetros (Conservador/Moderado/Agressivo).

---

### Referências rápidas

**DI em `MauiProgram.cs`**
```csharp
builder.Services.AddSingleton<IBrownianService, BrownianService>();
builder.Services.AddSingleton<BrownianViewModel>();
builder.Services.AddSingleton<BrownianPage>();
```

**Ligação do desenho na página**
```csharp
ChartView.Drawable = vm.ChartDrawable;
vm.RequestRedraw += (_, __) => ChartView.Invalidate();
```
