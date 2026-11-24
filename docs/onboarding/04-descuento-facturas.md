# Descuento de facturas (FCE) — preparar y crear la orden

Para descontar una factura primero tiene que estar **negociable**, y para eso debe estar **depositada en el ADC** (Agente de Depósito Colectivo). Una factura recién **recepcionada y aceptada** todavía **NO es negociable** hasta que se deposita en el ADC. A partir de que queda "en negociación", el flujo de ofertas es común → ver "Descuento — Negociación y ofertas (común)".

## Requisitos para descontar
Para que una FCE se pueda descontar tiene que estar **aceptada (expresa o tácita) y SIN cancelación**, en **ADC**, y con plazo de crédito **mayor a 21 días**. La FCE se negocia **sin recurso** (no garantizás el pago). (Detalle en "FCE — Régimen, estados y ciclo".)

## ADC y Auto ADC
- **Depositar en el ADC**: es lo que hace que una factura aceptada pase a ser **negociable**. Técnicamente, la plataforma **cambia la opción de transferencia de SCA a ADC**, deposita la FCE en **Caja de Valores** y **sincroniza el dato con AFIP** automáticamente (no necesitás entrar a AFIP).
- **Auto ADC (configuración OPCIONAL)**: la PYME puede **activar** Auto ADC para que sus facturas aceptadas se **depositen automáticamente en el ADC**, y queden negociables **sin tener que depositarlas a mano** cada vez. No es un paso obligatorio del descuento: es un setting que te ahorra ese paso. Si lo tenés activado, tus facturas aparecen directamente como negociables.

## Menú de Facturas
- **Recepcionadas**: FCEs recepcionadas/aceptadas en AFIP por la Gran Empresa Receptora. Si todavía no están en ADC, **no son negociables**.
- **Negociables / Depositadas**: ya depositadas en el ADC, listas para descontar.
- **Descontadas**: las que se descontaron en la plataforma.
- **No negociables**: no se pueden negociar (canceladas, vencidas o rechazadas).

## Caso A — la factura está en "Recepcionadas" (aceptada, pero todavía no en ADC)
Pasa cuando aceptaste la factura hace poco y no la depositaste en el ADC (y no tenés Auto ADC activado).
1. Ingresá a la plataforma y, si tenés más de una empresa, elegí con cuál operar.
2. Entrá a **Facturas recepcionadas**.
3. La factura aceptada todavía no es negociable: **ponela / depositala en el ADC** para que pase a **negociable**. (Si tenés **Auto ADC** activado, este paso se hace solo.)
4. Una vez negociable, **activá la negociación** (botón "Negociar" / "Activar negociación") y **confirmá**: eso **crea la orden de descuento** y la factura queda **en negociación**.
5. Al publicarse, **bancos e inversores ofertan y cotizan** tu factura. → Seguí en "Descuento — Negociación y ofertas (común)".

## Caso B — la factura ya está depositada / negociable
Pasa cuando tenés **Auto ADC** activado, o cuando te interesa una factura que **ya está depositada en el ADC**.
1. Andá directo a **Facturas negociables**.
2. Seleccioná la factura (o abrí su detalle).
   - En desktop: desde el menú lateral. En mobile: desde el acceso a **Facturas** en la home.
3. **Activá la negociación** ("Negociar" / "Activar negociación") y **confirmá**: se **crea la orden de descuento** y la factura queda **en negociación**. → Seguí en "Descuento — Negociación y ofertas (común)".

## Negociación múltiple
Para negociar varias facturas de crédito a la vez: seleccioná cada una y usá **"Activar negociación"**; al confirmar, se crea **una orden de descuento por cada FCE**.
