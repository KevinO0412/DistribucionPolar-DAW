# Lógica de negocio

Cada regla que implementa el sistema, por qué existe y en qué archivo vive.
Las rutas son relativas a la raíz del repositorio. Las cifras de los ejemplos están verificadas por pruebas
automatizadas (`tests/UnitTests/Domain`, `tests/IntegrationTests`) y por la colección de Postman.

## 1. Contexto

La distribuidora vende al mayor alimentos, bebidas y productos de limpieza a bodegas, abastos y comercios.

- **Mercancía:** se maneja por **cajas** (con un número fijo de unidades) y por **unidades sueltas**.
- **Almacén:** se guarda en un **galpón** dividido en depósitos y se despacha desde el galpón o desde **camiones de reparto**, que llevan su propio inventario.
- **Pagos:** los clientes compran a crédito o pagan parte al momento (**abono**), así que cada cliente tiene un **saldo**.
- **Envases retornables (vacíos):** algunas bebidas los usan, y el cliente debe devolverlos.

## 2. Convenciones del dominio

### 2.1 Cajas y unidades

| Regla | Por qué | Dónde |
| --- | --- | --- |
| El stock se guarda **siempre en unidades** | Una sola unidad de medida: vender 1 caja o 36 unidades descuenta lo mismo | `Product.StockUnits`, `TruckStock.StockUnits` |
| `unidades = cajas × unidadesPorCaja + sueltas` | Así se cuenta la mercancía | `BoxQuantity.ToUnits` |
| `cajas = unidades ÷ unidadesPorCaja` (entera); `sueltas = resto` | Para mostrarlo como lo entiende el personal | `BoxQuantity.FromUnits` |
| Formato `"2 cajas + 5 und"`, `"1 caja"`, `"5 und"` | Lectura del almacén | `BoxQuantity.ToString` |
| Cajas y sueltas nunca son negativas, ni ambas 0 en una operación | Una cantidad vacía o negativa no tiene sentido físico | `BoxQuantity`, validadores |

**Ejemplo:** con 36 unidades por caja, 2 cajas + 5 und = 77 und, y 77 und = 2 cajas + 5 und.

### 2.2 Precios

| Regla | Dónde |
| --- | --- |
| Precio por caja y precio por unidad son **independientes**: la unidad suelta no se deduce de la caja | `Product.PriceBox`, `Product.PriceUnit` |
| `subtotal = cajas × precioCaja + sueltas × precioUnidad`. Se aplica en facturas, compras, averías, consumos y consignación | `Product.CalculateSubtotal` |
| `valorInventario = unidades × precioUnidad` | `Product.InventoryValue`, `ReportQueries` |
| Montos en `decimal`, guardados como `numeric(18,2)`, con un máximo de 2 decimales | configuraciones Fluent API, validadores |

**Ejemplo:** 2 cajas a $23,00 + 5 und a $0,64 = **$49,20**.

### 2.3 Saldos con signo

Los saldos del cliente (dinero, cajas de vacíos y unidades de vacíos) llevan signo:

| Valor | Significado | Texto |
| --- | --- | --- |
| negativo | el cliente **debe** | `Debe: $60.00` · `Debe vacíos: 1 cajas + 5 und` |
| positivo | saldo **a favor** del cliente | `Saldo a favor: $20.00` · `Vacíos a favor: …` |
| cero | sin saldo | `Sin saldo` · `Sin vacíos pendientes` |

- **Dinero en la calle** = Σ deudas de los clientes con saldo negativo. Lo mismo vale para los vacíos.
- **Cuentas por cobrar** = clientes con cualquiera de los tres saldos negativos.

Dónde: `Client`, `BusinessMappings.MoneyStatus/EmptiesStatus`, `ReportQueries.GetStreetBalancesAsync`.

### 2.4 Ubicaciones de inventario

- **Galpón:** `Product.StockUnits`.
- **Camión:** cada camión tiene sus líneas `(producto, unidades)`; una línea en 0 se elimina.
- **Inventario total** = galpón + todos los camiones.

### 2.5 "Hoy"

Los reportes y la numeración mensual usan la **fecha local de la empresa** (`Business:TimeZone`, por defecto
`America/Caracas`), no la fecha UTC del servidor. Dónde: `BusinessClock`.

## 3. Catálogo

| Regla | Dónde |
| --- | --- |
| 3 categorías, cada una con su depósito: Alimentos → Depósito #1, Bebidas → Depósito #2, Jabones/P&G → Depósito #3 | `CatalogSeedData` |
| Nombre de categoría único, sin distinguir mayúsculas; no se elimina una categoría con productos | `CategoryService`, FK `Restrict` |
| Producto: nombre y marca obligatorios; precios > 0; costo ≥ 0; unidades por caja > 0; mínimo ≥ 0; máximo > mínimo | `Product`, validadores, check constraints |
| El **SKU** se genera solo (`CAT-PRO-0000`) y es único; la secuencia cuenta también los eliminados | `CorporateSkuGenerator`, `ProductService` |
| Editar un producto **no** cambia el stock | `UpdateProductRequest` |
| **Salud del stock**: agotado / riesgo crítico / bajo / óptimo / lleno / exceso, con compra sugerida | `ProductHealthEvaluator` |
| **Vacíos**: solo generan vacíos los productos con grupo de envase (`RET_222ML_X36`, `RET_POLAR_PILSEN_330ML_X24`, `PEPSI_125L_X6`, `PEPSI_350ML_X24`) | `EmptyReturnGroups`, `Product.EmptyGroupKey` |
| **Autorizado para regalía**: es un dato del producto (`IsGiftEligible`), no una regla escrita sobre nombres | `Product.IsGiftEligible` |

En los datos iniciales, `IsGiftEligible` se marcó con la regla del negocio. Cumplen la regla:
- Las cervezas retornables de la categoría Bebidas cuyo nombre o marca contiene POLAR, SOLERA o CERVEZA (excepto las maltas).
- La malta retornable de 222 ml.
- Las Pepsi de 2 L x6.

## 4. Inventario: galpón y camiones

| Operación | Regla | Quién | Dónde |
| --- | --- | --- | --- |
| **Cargar camión (modo agregar)** | Descuenta del galpón y suma al camión la cantidad indicada; valida el stock del galpón | Admin, Employee | `TruckService.LoadAsync` |
| **Cargar camión (modo meta)** | La cantidad es la **meta final**: carga `meta − actual`. Si el camión ya tiene esa cantidad o más: *"El camión ya tiene esa cantidad o más"* | Admin, Employee | `TruckService.LoadAsync` |
| **Transferir / desmontar** | Del camión al galpón o a otro camión; no más de lo que hay en el camión | Admin, Employee | `TruckService.TransferAsync` |
| **Historial de cargas** | Cada entrada a un camión guarda la cantidad, el stock antes y después, el origen (galpón, otro camión o reverso de anulación) y el usuario | — | `TruckLoad` |
| **Ajuste manual del camión** | Fija la cantidad absoluta (0 elimina la línea); motivo obligatorio; queda auditado | Admin | `TruckService.AdjustAsync` |
| **Vaciar camión** | Devuelve **toda** la mercancía al galpón | Admin | `TruckService.UnloadAsync` |
| **Ajuste manual del galpón** | Fija el stock absoluto; motivo obligatorio; queda auditado | Admin | `InventoryService` |
| **Consultas** | Inventario por galpón, camión y categoría (unidades y valor); en qué camiones está un producto | Admin, Employee | `ReportQueries` |

**Ejemplos (modo meta):**
- El camión tiene 10 und y la meta es 36: carga **26 und**.
- El camión tiene 40 und y la meta es 36: **se rechaza**.

## 5. Clientes y estado de cuenta

| Regla | Dónde |
| --- | --- |
| Alta: RIF/cédula y nombre obligatorios; RIF único; dirección por defecto *"Cliente ocasional / sin dirección registrada"*; tipo `Ocasional`; saldos en 0 | `Client`, `ClientService.CreateAsync` |
| **Código correlativo** (001, 002…) asignado de forma atómica en la base, nunca repetido | `DocumentNumberGenerator` |
| **Código, nombre y RIF son inmutables**; solo se editan dirección, referencia y tipo | `Client.UpdateContact` |
| Búsqueda por nombre, RIF, dirección o referencia, sin distinguir mayúsculas | `ClientRepository.SearchAsync` |
| **Abono de dinero**: monto > 0; suma al saldo | `Client.ApplyPayment` |
| **Devolución de vacíos** fuera de factura, por grupo: no negativos, al menos uno > 0; suma al saldo del grupo y al total | `Client.ApplyEmptiesMovement` |
| **Ajuste manual de saldos** (solo Admin): valores absolutos con signo, motivo obligatorio, antes y después en la auditoría | `Client.AdjustBalances` |
| Eliminar (solo Admin) es borrado lógico y **solo sin saldos pendientes** | `ClientService.DeleteAsync` |
| **Estado de cuenta**: movimientos inmutables, del más reciente al más antiguo, en pestañas (facturas, regalías, abonos, vacíos, ajustes) | `ClientMovement`, `ClientService.GetStatementAsync` |
| **Saldo de vacíos por grupo** guardado como dato estructurado (no reconstruido desde textos) | `ClientEmptyBalance` |
| Última compra = fecha de la última factura de venta vigente | `ClientRepository.GetLastSaleDatesAsync` |

**Ejemplo:** saldo −60 y abona 25 → saldo **−35**.

## 6. Facturación

### 6.1 Factura de venta

Se puede despachar desde el **camión** o desde el **galpón**. Todo ocurre en **una sola transacción**: si un paso
falla, no cambia nada (ni inventario, ni saldos, ni numeración).

1. **Validación:**
   - Cliente existente, productos activos y abono ≥ 0.
   - Camión obligatorio si el origen es Camión.
   - Vacíos devueltos no negativos.
   - Cada producto aparece una sola vez.
2. **Stock:** se verifica el stock de **todas** las líneas en el origen antes de descontar. Si falta en alguna, el error lista todas las faltas.
3. **Descuento** del inventario del origen.
4. **Número:** `Factura {Mes} {Año} #n`, una secuencia **mensual** independiente para facturas y regalías. El año evita choques entre octubre de un año y el siguiente.
5. **Totales:**
   - `total = Σ subtotales`
   - `pendiente = total − abono`. Si el abono supera el total, el pendiente es negativo y queda a favor del cliente.
6. **Cliente:** `saldo −= pendiente`.
7. **Vacíos:**
   - Por cada grupo: `generados = Σ cajas` y `Σ sueltas` vendidas del grupo (una caja vendida = una caja de vacío).
   - `pendiente del grupo = generados − devueltos`.
   - `saldo de vacíos −= pendiente`, por grupo y en total.
   - Cajas y unidades se llevan por separado; no se convierten unidades en cajas.
8. **Estado de cuenta:**
   - Movimiento *factura*: `Total | Abono | Pendiente`.
   - Movimiento *abono*, si lo hubo.
   - Movimiento *vacíos*, con el detalle por grupo y el saldo resultante.
9. **"Quién atiende":**
   - Si el usuario **puede cambiar de vendedor**, elige un empleado activo (o se usa su vendedor por defecto).
   - Si **no puede**, se usa siempre su vendedor fijo; si intenta elegir otro, se rechaza.
10. Cada línea guarda una **copia del producto** (nombre, SKU, precios, unidades por caja), así que cambiar precios después no altera las facturas emitidas.

**Ejemplos verificados:**

| Caso | Resultado |
| --- | --- |
| Saldo 0, total 100, abono 40 | pendiente 60, saldo **−60** |
| Saldo 0, total 100, abono 120 | saldo **+20** (a favor) |
| Stock de 10 und y se piden 11 | error *Inventario insuficiente*, **sin ningún cambio** y sin consumir número de factura |
| Vende 3 cajas + 5 und de un producto del grupo 222 ml y el cliente devuelve 2 cajas | vacíos **−1 caja, −5 und** |

### 6.2 Regalía a cliente

Igual que la factura, con estas diferencias:
- Solo productos **autorizados para regalía**.
- Total, abono y pendiente = 0; **no afecta el saldo de dinero**.
- **Sí genera vacíos**, y no se reciben devoluciones en la misma regalía.
- El vendedor es el del usuario conectado.
- La observación es *"Regalía a cliente - productos autorizados"*.
- Numeración propia: `Regalía {Mes} {Año} #n`.

### 6.3 Anulación (solo Admin)

Requiere motivo y no se puede anular dos veces. En **una sola transacción**:

1. **Inventario:**
   - Devuelve las unidades de cada línea al **origen**.
   - Si el origen es un camión, queda registrado en su historial como *"Reverso por anulación"*.
   - Si el camión ya no existe, la anulación falla sin cambios.
2. **Dinero:** `saldo += pendiente`. Se revierte solo el pendiente; lo cobrado no se convierte en saldo a favor.
3. **Vacíos:** `saldo de vacíos += (generados − devueltos)`, por grupo y en total.
4. Marca la factura como anulada (motivo, fecha y usuario) y registra el movimiento *"Factura anulada"* / *"Regalía anulada"*.
5. Las facturas anuladas **se excluyen de todos los reportes**.

**Ejemplo:** una factura con pendiente 60 y vacíos −1/−5, al anularse, devuelve el saldo a +60 sobre el valor que tenía, los vacíos a +1/+5 y el inventario a su origen.

## 7. Compras, averías y consumos (solo Admin)

| Proceso | Regla | Dónde |
| --- | --- | --- |
| **Compra / entrada** | Empleado activo obligatorio; tipo de documento (factura, nota de entrega, entrada manual, ajuste); suma el stock al galpón; número `Compra #0001`; totales de unidades y monto (a precios actuales) | `PurchaseService`, `Purchase` |
| **Avería** | Origen galpón o camión; tipo (averiado, no apto, desechado), motivo (vencido, roto, golpeado, derramado, mojado, empaque abierto, otro) y acción (desechado, consumo interno, regalado, devuelto al proveedor); descuenta el stock; si no alcanza, *Inventario insuficiente* y no se registra; **pérdida estimada = subtotal** | `StockWithdrawalService`, `DamagedProduct` |
| **Consumo interno / regalía a empleado** | Igual que la avería, con empleado activo obligatorio; **valor estimado = subtotal** | `InternalConsumption` |

**Ejemplo:** avería de 3 und a $0,64 = pérdida de **$1,92**.

## 8. Consignación

| Paso | Regla | Dónde |
| --- | --- | --- |
| Crear evento | Nombre y responsable obligatorios; productos del **galpón**; si un producto se repite, se acumula; **se valida y descuenta el stock en la misma transacción** | `ConsignmentService.CreateAsync` |
| Devolución | Solo con el evento abierto; no puede superar lo pendiente por devolver; reintegra el stock al galpón | `ConsignmentEvent.RegisterReturn` |
| Cierre | Ya no admite cambios; se cobra **solo lo vendido** | `ConsignmentEvent.Close` |
| Cálculo | `vendidas = entregadas − devueltas`; `monto = cajasVendidas × precioCaja + sueltasVendidas × precioUnidad`, con los precios del momento de la entrega | `ConsignmentItem` |

**Ejemplo:** entrega 2 cajas de 24 (48 und) y devuelve 30 und, así que se vendieron 18 und = 0 cajas + 18 und × $0,46 = **$8,28**.

## 9. Reportes (solo Admin)

Siempre se excluyen las facturas anuladas. "Hoy" es el día local de la empresa.

| Indicador | Fórmula |
| --- | --- |
| Ventas del día | Σ total de facturas de venta |
| N.º de facturas / regalías | conteo por tipo |
| Cobrado en facturas | Σ abono de facturas de venta |
| Fiado generado | Σ pendiente de facturas de venta |
| Abonos del día | Σ movimientos de abono (incluye los abonos al facturar) |
| Vacíos generados / devueltos | Σ de las facturas y regalías del día |
| Consumo interno / pérdida por averías | Σ valor estimado del día |
| Balance | dinero y vacíos en la calle, clientes por cobrar, inventario (galpón + camiones, en unidades y valor), consignación por cobrar |

Dónde: `ReportService`, `ReportQueries`. Las agregaciones se calculan en PostgreSQL.

Documentos y respaldo:
- **Ticket PDF** de factura o regalía: empresa, número, fecha, cliente, despacho, productos, totales, observación de pago, vacíos, líneas de firma y *"Gracias por su compra"*.
- **PDF de liquidación** de consignación.
- **Respaldo** en JSON (todas las tablas) y en CSV (productos, clientes, facturas, movimientos).

Dónde: `QuestPdfGenerator`, `BackupExporter`.

## 10. Usuarios, roles y auditoría

| Regla | Dónde |
| --- | --- |
| Se entra con username o email + clave (mínimo 6). Credenciales incorrectas dan siempre *"Usuario o clave incorrectos."*; un usuario inactivo da *"Este usuario está inactivo."* | `AuthService` |
| Solo dos roles: **Admin** (gestión total) y **Employee** (operación) | `UserRole` |
| Admin crea y edita usuarios y restablece claves; no puede desactivarse ni cambiar su propio rol | `UserService` |
| Cada usuario puede cambiar su propia clave (debe indicar la actual) | `UserService.ChangeOwnPasswordAsync` |
| Cada usuario tiene un **vendedor fijo** y un permiso para **elegir vendedor** al facturar | `User.AttendantName`, `User.CanChangeAttendant` |
| Empleados: no se borran, se desactivan; la lista operativa muestra solo los activos | `Employee` |
| **Auditoría**: cada proceso que cambia datos registra la acción, el módulo, la entidad, el monto, el antes y el después (JSONB), el usuario, el rol y la IP. Si la auditoría falla, el proceso **no** se revierte | `AuditService`, `AuditLog` |

Permisos por operación: ver [Arquitectura §7](02-ARQUITECTURA.md#7-seguridad).

## 11. Tasas de cambio

Los montos se manejan en dólares, pero el cobro puede recibirse en otras monedas (la observación de pago lo
registra). `GET /api/exchange-rates` consulta un proveedor externo de tasas del dólar (VES, COP, EUR):
- Lo protege con **reintentos, timeout y Circuit Breaker**.
- Si el proveedor no responde, devuelve el último valor conocido; si no hay ninguno, responde 503.

Dónde: `ExchangeRateProvider`.

## 12. Datos iniciales

| Dato | Detalle |
| --- | --- |
| Categorías | 3, con su depósito |
| Productos | 275 del catálogo de referencia (143 Alimentos, 49 Bebidas, 83 Jabones/P&G): 9 con grupo de vacíos y 6 autorizados para regalía |
| Valores de demostración | El catálogo no trae costo ni niveles de stock. Valores ficticios: costo = 80% del precio por caja; mínimo = 5 cajas; máximo = 40 cajas; stock entre 0 y 44 cajas |
| Empleados, camiones, clientes | 7 empleados, 6 camiones y 5 clientes, todos **ficticios** |
| Usuarios | Según el entorno (ver [Arquitectura §9](02-ARQUITECTURA.md#9-entornos-contenedores-y-ci)) |

Las unidades por caja se calcularon como `precioCaja ÷ precioUnidad` redondeado y se verificaron contra el nombre
del producto. Los precios por unidad con más de dos decimales se redondearon a 2.
