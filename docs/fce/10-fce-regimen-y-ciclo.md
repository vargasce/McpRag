# FCE — Régimen, estados y ciclo (para entender el descuento)

Esto es el **marco operativo/regulatorio** del descuento de Facturas de Crédito Electrónicas (FCE), para entender el *por qué* detrás de los pasos. (Las FCE se negocian **sin recurso**: la PYME no garantiza el pago; el riesgo de cobro está en la Gran Empresa Receptora.)

## Opciones de transferencia: SCA vs ADC
Al emitir la FCE en AFIP, la PYME elige **obligatoriamente** una **opción de transferencia**:
- **SCA — Sistema de Circulación Abierta**: la FCE circula por el **mercado bancario / homebanking** (COELSA). Al vencimiento se cobra por débito automático al CBU de la Gran Empresa. **No es negociable en Invoitrade.**
- **ADC — Agente de Depósito Colectivo**: la FCE se deposita en **Caja de Valores**. **Es la opción que permite negociarla/descontarla en Invoitrade.**

**Importante:** para descontar en Invoitrade, la FCE debe estar en **ADC**. La opción se puede **cambiar de SCA a ADC** siempre que la FCE **todavía no esté aceptada**. Desde la plataforma, al "depositar en ADC" (o con **Auto ADC**), Invoitrade **cambia la opción a ADC, deposita la FCE en Caja de Valores y sincroniza el dato con AFIP automáticamente** — la PYME no necesita entrar a AFIP.

## Estados de la FCE en AFIP
1. **Pendiente**: la PYME emitió la FCE.
2. **Recepcionada**: AFIP la notificó al domicilio fiscal de la Gran Empresa.
3. La Gran Empresa tiene **21 días corridos** (plazo actualizable por AFIP) desde la recepción para:
   - **Aceptarla** (aceptación **expresa**),
   - **Rechazarla** (con un motivo), o
   - **Cancelarla** (pagarla).
   - Si no hace nada en esos 21 días → **Aceptada tácitamente** (AFIP la acepta automáticamente).
4. **Informada a Agente de Depósito**: una vez aceptada y depositada en Caja de Valores (ADC), queda lista para negociarse.

## Requisitos para poder descontarla
- Debe estar **aceptada (expresa o tácita) y SIN cancelación** (si la Gran Empresa la canceló/pagó, no se descuenta).
- Debe estar en **ADC** (depositada en Caja de Valores).
- El **plazo de crédito debe ser mayor a 21 días** para ser negociable.

## Notas de Crédito / Débito (NC/ND MiPyME)
Si hay notas de crédito o débito asociadas a la FCE, **ajustan el monto neto negociable**: monto a negociar = **FCE − NC + ND**. Se aceptan/rechazan junto con la factura.

## Retenciones impositivas
Las **practica la Gran Empresa Receptora** (no el inversor). Si la FCE se acepta **tácitamente**, AFIP detrae automáticamente, para ajustar el monto a negociar, aproximadamente: **15% Nación, 4% Provincias + CABA, 1% Municipios** *(porcentajes actualizables)*. Por eso el **monto neto a negociar** puede ser menor al total de la factura.

## Sincronización con AFIP
Invoitrade **captura automáticamente** tus FCE de AFIP y muestra sus estados (pendientes, recepcionadas, aceptadas, rechazadas, informadas a agente de depósito). **No necesitás entrar a la página de AFIP** para consultarlas ni para depositarlas en ADC.

## Qué pasa al vencimiento de la FCE
- Las FCE depositadas en **ADC** se pagan al vencimiento a **Caja de Valores**.
- Caja de Valores identifica al **último tenedor** y le paga:
  - al **inversor**, si la factura **se descontó/vendió**;
  - a la **PYME**, si **no se vendió** (en ese caso entra el flujo de **gestión de cobro**, ver guía correspondiente).
- El proceso de entrega-contra-pago (DVP) lo coordina el **custodiante** (la entidad que tiene la custodia de la FCE): cuando aceptás una oferta, deposita la FCE en la cuenta del inversor y te paga el adelanto a vos.

## Por qué descontar una FCE (beneficios)
- **Mejores tasas**: el riesgo está concentrado en la capacidad de pago de la Gran Empresa.
- **No ocupa líneas bancarias**.
- Es **sin recurso**: la PYME no queda obligada si la Gran Empresa no paga.
