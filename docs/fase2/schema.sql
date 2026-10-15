CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    CREATE TABLE "Categories" (
        "Id" uuid NOT NULL,
        "Name" character varying(50) NOT NULL,
        "Description" character varying(250),
        "Deposit" character varying(100) NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "LastModifiedAt" timestamptz,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_Categories" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "Username" character varying(50) NOT NULL,
        "Email" character varying(150) NOT NULL,
        "FullName" character varying(100) NOT NULL,
        "PasswordHash" character varying(256) NOT NULL,
        "Role" character varying(20) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "LastModifiedAt" timestamptz,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    CREATE TABLE "Products" (
        "Id" uuid NOT NULL,
        "Name" character varying(150) NOT NULL,
        "Brand" character varying(80) NOT NULL,
        "SKU" character varying(20) NOT NULL,
        "CategoryId" uuid NOT NULL,
        "PriceBox" numeric(18,2) NOT NULL,
        "PriceUnit" numeric(18,2) NOT NULL,
        "CostPrice" numeric(18,2) NOT NULL,
        "UnitsPerBox" integer NOT NULL,
        "StockUnits" integer NOT NULL DEFAULT 0,
        "MinStock" integer NOT NULL,
        "MaxStock" integer NOT NULL,
        "IsReturnable" boolean NOT NULL DEFAULT FALSE,
        "EmptyGroupKey" character varying(50),
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "LastModifiedAt" timestamptz,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_Products" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_Products_CostPrice_NotNegative" CHECK ("CostPrice" >= 0),
        CONSTRAINT "CK_Products_MaxStock_GreaterThan_MinStock" CHECK ("MaxStock" > "MinStock"),
        CONSTRAINT "CK_Products_PriceBox_Positive" CHECK ("PriceBox" > 0),
        CONSTRAINT "CK_Products_PriceUnit_Positive" CHECK ("PriceUnit" > 0),
        CONSTRAINT "CK_Products_StockUnits_NotNegative" CHECK ("StockUnits" >= 0),
        CONSTRAINT "CK_Products_UnitsPerBox_Positive" CHECK ("UnitsPerBox" > 0),
        CONSTRAINT "FK_Products_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Categories" ("Id", "CreatedAt", "Deposit", "Description", "LastModifiedAt", "Name")
    VALUES ('a1111111-1111-4111-8111-111111111111', TIMESTAMPTZ '2026-10-01T00:00:00Z', 'Depósito #1 - Alimentos', 'Víveres, granos, harinas, café, enlatados y alimentos para mascotas', NULL, 'Alimentos');
    INSERT INTO "Categories" ("Id", "CreatedAt", "Deposit", "Description", "LastModifiedAt", "Name")
    VALUES ('b2222222-2222-4222-8222-222222222222', TIMESTAMPTZ '2026-10-01T00:00:00Z', 'Depósito #2 - Bebidas', 'Cervezas, maltas, refrescos y aguas; incluye envases retornables', NULL, 'Bebidas');
    INSERT INTO "Categories" ("Id", "CreatedAt", "Deposit", "Description", "LastModifiedAt", "Name")
    VALUES ('c3333333-3333-4333-8333-333333333333', TIMESTAMPTZ '2026-10-01T00:00:00Z', 'Depósito #3 - Jabones/P&G', 'Detergentes, limpieza del hogar y cuidado personal', NULL, 'Jabones/P&G');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000001', 'Polar Light', 'b2222222-2222-4222-8222-222222222222', 18.4, TIMESTAMPTZ '2026-10-01T00:00:00Z', 'RET_222ML_X36', TRUE, TRUE, NULL, 1440, 180, 'POLAR LIGHT RET 222MLx36UN', 23.0, 0.64, 'BEB-POL-0001', 252, 36);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000002', 'Polar Light', 'b2222222-2222-4222-8222-222222222222', 16.32, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'POLAR LIGHT LAT 250MLx24UN', 20.4, 0.85, 'BEB-POL-0002', 336, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000003', 'Polar Light', 'b2222222-2222-4222-8222-222222222222', 19.22, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'POLAR LIGHT LAT SLEEK 355MLx24UN', 24.02, 1.0, 'BEB-POL-0003', 504, 24);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000004', 'Solera Light', 'b2222222-2222-4222-8222-222222222222', 18.4, TIMESTAMPTZ '2026-10-01T00:00:00Z', 'RET_222ML_X36', TRUE, TRUE, NULL, 1440, 180, 'SOLERA LIGHT RET 222MLx36UN', 23.0, 0.64, 'BEB-SOL-0004', 1008, 36);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000005', 'Solera Light', 'b2222222-2222-4222-8222-222222222222', 16.32, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'SOLERA LIGHT LAT 250MLx24UN', 20.4, 0.85, 'BEB-SOL-0005', 840, 24);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000006', 'Maltin Polar', 'b2222222-2222-4222-8222-222222222222', 13.9, TIMESTAMPTZ '2026-10-01T00:00:00Z', 'RET_222ML_X36', TRUE, TRUE, NULL, 1440, 180, 'MALTIN POLAR RET 222MLx36UN', 17.38, 0.48, 'BEB-MAL-0006', 1512, 36);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000007', 'Maltin Polar', 'b2222222-2222-4222-8222-222222222222', 14.06, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'MALTIN POLAR LAT 250MLx24UN', 17.57, 0.73, 'BEB-MAL-0007', 96, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000008', 'Maltin Polar', 'b2222222-2222-4222-8222-222222222222', 7.87, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'MALTIN POLAR PET 1.5 Lx6UN', 9.84, 1.64, 'BEB-MAL-0008', 66, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000009', 'Maltin Polar', 'b2222222-2222-4222-8222-222222222222', 6.24, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'MALTIN POLAR NR 250MLx12UN', 7.8, 0.65, 'BEB-MAL-0009', 216, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000010', 'Maltin Polar', 'b2222222-2222-4222-8222-222222222222', 17.11, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'MALTIN POLAR LAT SLEEK 355MLx24UN', 21.39, 0.89, 'BEB-MAL-0010', 600, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000011', 'Solera', 'b2222222-2222-4222-8222-222222222222', 18.44, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'SOLERA BOHEMIA LATA 250MLX24UN', 23.05, 0.96, 'BEB-SOL-0011', 768, 24);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000012', 'Solera', 'b2222222-2222-4222-8222-222222222222', 20.7, TIMESTAMPTZ '2026-10-01T00:00:00Z', 'RET_222ML_X36', TRUE, TRUE, NULL, 1440, 180, 'SOLERA RET 222MLx36UN', 25.87, 0.72, 'BEB-SOL-0012', 1404, 36);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000013', 'Sangria Caroreña', 'b2222222-2222-4222-8222-222222222222', 30.87, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'SANGRIA CAROREÑA TINTA 1,75Lx6UN', 38.59, 6.43, 'BEB-SAN-0013', 6, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000014', 'Sangria Caroreña', 'b2222222-2222-4222-8222-222222222222', 30.87, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'SANGRIA CAROREÑA BLANCA 1,75Lx6UN', 38.59, 6.43, 'BEB-SAN-0014', 48, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000015', 'Sangria Caroreña', 'b2222222-2222-4222-8222-222222222222', 30.87, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'SANGRIA CAROREÑA ROSADA 1,75Lx6UN', 38.59, 6.43, 'BEB-SAN-0015', 90, 6);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000016', 'Sangria Caroreña', 'b2222222-2222-4222-8222-222222222222', 23.04, TIMESTAMPTZ '2026-10-01T00:00:00Z', 'RET_222ML_X36', TRUE, TRUE, NULL, 1440, 180, 'SANGRIA CAROREÑA VERANO RET222MLx36UN', 28.8, 0.8, 'BEB-SAN-0016', 792, 36);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000017', 'Sangria Caroreña', 'b2222222-2222-4222-8222-222222222222', 24.32, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'SANGRIA CAROREÑA LATA 250MLx24UN', 30.4, 1.27, 'BEB-SAN-0017', 696, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000018', 'Sangria Caroreña', 'b2222222-2222-4222-8222-222222222222', 14.73, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'SANGRIA CAROREÑA VERANO MOJITO LAT355x12', 18.41, 1.53, 'BEB-SAN-0018', 432, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000019', 'Sangria La Que Manda', 'b2222222-2222-4222-8222-222222222222', 22.9, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'SANGRIA LA QUE MANDA TINTA 1,75LX6UN', 28.63, 4.77, 'BEB-SAN-0019', 258, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000020', 'Sangria La Que Manda', 'b2222222-2222-4222-8222-222222222222', 18.43, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'SANGRIA LA QUE MANDA BLANCA LATA 250MLX24UN', 23.04, 0.96, 'BEB-SAN-0020', 120, 24);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000021', 'Polar Pilsen', 'b2222222-2222-4222-8222-222222222222', 18.4, TIMESTAMPTZ '2026-10-01T00:00:00Z', 'RET_222ML_X36', TRUE, TRUE, NULL, 1440, 180, 'POLAR PILSEN RET 222MLx36UN', 23.0, 0.64, 'BEB-POL-0021', 432, 36);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000022', 'Polar Pilsen', 'b2222222-2222-4222-8222-222222222222', 16.43, TIMESTAMPTZ '2026-10-01T00:00:00Z', 'RET_POLAR_PILSEN_330ML_X24', TRUE, TRUE, NULL, 960, 120, 'POLAR PILSEN RET 330MLx24UN', 20.54, 0.86, 'BEB-POL-0022', 456, 24);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000023', 'Polar Pilsen', 'b2222222-2222-4222-8222-222222222222', 16.32, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'POLAR PILSEN LAT 250MLx24UN', 20.4, 0.85, 'BEB-POL-0023', 624, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000024', 'Polar Pilsen', 'b2222222-2222-4222-8222-222222222222', 19.22, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'POLAR PILSEN LAT SLEEK 355MLx24UN', 24.02, 1.0, 'BEB-POL-0024', 792, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000025', 'Minalba', 'b2222222-2222-4222-8222-222222222222', 12.7, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'MINALBA AGUA PET 355MLx24UN', 15.87, 0.66, 'BEB-MIN-0025', 960, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000026', 'Minalba', 'b2222222-2222-4222-8222-222222222222', 16.11, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'MINALBA AGUA PET TR 600MLx24UN', 20.14, 0.84, 'BEB-MIN-0026', 48, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000027', 'Minalba', 'b2222222-2222-4222-8222-222222222222', 7.35, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 80, 10, 'MINALBA AGUA PET S/G 5Lx2UN', 9.19, 4.59, 'BEB-MIN-0027', 18, 2);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000028', 'Minalba', 'b2222222-2222-4222-8222-222222222222', 10.96, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'MINALBA SPARKLYN 500MLX12UN', 13.7, 1.14, 'BEB-MIN-0028', 192, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000029', 'Minalba', 'b2222222-2222-4222-8222-222222222222', 14.96, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'MINALBA AGUA PET S/G 1,5Lx12UN', 18.7, 1.56, 'BEB-MIN-0029', 276, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000030', 'Gatorade', 'b2222222-2222-4222-8222-222222222222', 16.99, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'GATORADE PET 500MLx12UN', 21.24, 1.77, 'BEB-GAT-0030', 360, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000031', 'Rockstar', 'b2222222-2222-4222-8222-222222222222', 16.96, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'ROCKSTAR LATA 355MLx24UN', 21.2, 0.88, 'BEB-ROC-0031', 888, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000032', 'Yukery', 'b2222222-2222-4222-8222-222222222222', 19.0, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'YUKERY NARANJADA S/A PET 1,5Lx6UN', 23.75, 3.96, 'BEB-YUK-0032', 264, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000033', 'Yukery', 'b2222222-2222-4222-8222-222222222222', 19.0, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'YUKERY NARANJADA PET 1,5Lx6UN', 23.75, 3.96, 'BEB-YUK-0033', 36, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000034', 'Yuk.', 'b2222222-2222-4222-8222-222222222222', 8.43, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'YUK. DURAZNO C.P. BOT 250MLX12UN', 10.54, 0.88, 'BEB-YUK-0034', 156, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000035', 'Yuky-Pak', 'b2222222-2222-4222-8222-222222222222', 17.35, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'YUKY-PAK MANZANA LD 250MLx24UN', 21.69, 0.9, 'BEB-YUK-0035', 480, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000036', 'Pepsi / Sabores', 'b2222222-2222-4222-8222-222222222222', 5.6, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'SABORES 2LX6UN', 7.0, 1.17, 'BEB-SAB-0036', 162, 6);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000037', 'Pepsi / Sabores', 'b2222222-2222-4222-8222-222222222222', 3.6, TIMESTAMPTZ '2026-10-01T00:00:00Z', 'PEPSI_125L_X6', TRUE, TRUE, NULL, 240, 30, 'PEPSI/SABORES 1,25X6UN', 4.5, 0.75, 'BEB-PEP-0037', 204, 6);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000038', 'Pepsi / Sabores', 'b2222222-2222-4222-8222-222222222222', 3.61, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'PEPSI/SABORES 1LX6UN', 4.51, 0.75, 'BEB-PEP-0038', 246, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000039', 'Pepsi / Sabores', 'b2222222-2222-4222-8222-222222222222', 17.61, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'PEPSI/SABORES LATA 355X24UN', 22.01, 0.92, 'BEB-PEP-0039', 72, 24);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "IsReturnable", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000040', 'Pepsi / Sabores', 'b2222222-2222-4222-8222-222222222222', 8.8, TIMESTAMPTZ '2026-10-01T00:00:00Z', 'PEPSI_350ML_X24', TRUE, TRUE, NULL, 960, 120, 'PEPSI/SABORES 350X24UN', 11.0, 0.46, 'BEB-PEP-0040', 240, 24);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000041', 'Minalba', 'b2222222-2222-4222-8222-222222222222', 20.8, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'MINALBA SPARKLING SODA LAT355MLx24UN', 26.0, 1.08, 'BEB-MIN-0041', 408, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000042', 'Pan', 'a1111111-1111-4111-8111-111111111111', 21.92, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 800, 100, 'PAN HARINA MAIZ AMARILLA 1KGx20UN BOPP', 27.4, 1.37, 'ALI-PAN-0042', 480, 20);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000043', 'Pan', 'a1111111-1111-4111-8111-111111111111', 21.92, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 800, 100, 'PAN MEZCLA MAIZ BLANCO Y ARROZ 1KGx20UN', 27.4, 1.37, 'ALI-PAN-0043', 620, 20);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000044', 'Pan', 'a1111111-1111-4111-8111-111111111111', 19.84, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 800, 100, 'PAN HPM BLANCO GLUTEN FREE 1KGX20UN VE', 24.8, 1.24, 'ALI-PAN-0044', 760, 20);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000045', 'Pan', 'a1111111-1111-4111-8111-111111111111', 22.83, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PAN MEZCLA PARA CACHAPAS 500GRx12UN', 28.54, 2.38, 'ALI-PAN-0045', 12);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000046', 'Pan', 'a1111111-1111-4111-8111-111111111111', 22.83, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PAN SEMILLAS NUTRITIVAS 500Gx12UN', 28.54, 2.38, 'ALI-PAN-0046', 84, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000047', 'Pan', 'a1111111-1111-4111-8111-111111111111', 22.83, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PAN AREPITAS DULCE 500Gx12UN', 28.54, 2.38, 'ALI-PAN-0047', 168, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000048', 'Primor', 'a1111111-1111-4111-8111-111111111111', 38.4, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'PRIMOR ARROZ PERLADO 900Gx24UN', 48.0, 2.0, 'ALI-PRI-0048', 504, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000049', 'Primor', 'a1111111-1111-4111-8111-111111111111', 27.26, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'PRIMOR ARROZ TRADICIONAL 900Gx24UN', 34.08, 1.42, 'ALI-PRI-0049', 672, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000050', 'Primor', 'a1111111-1111-4111-8111-111111111111', 28.42, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'PRIMOR ARROZ CLÁSICO SUPERIOR 900Gx24UN', 35.52, 1.48, 'ALI-PRI-0050', 840, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000051', 'Primor', 'a1111111-1111-4111-8111-111111111111', 17.57, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PRIMOR PASTA LARGA LINGUINI 1KGx12UN', 21.96, 1.83, 'ALI-PRI-0051', 504, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000052', 'Primor', 'a1111111-1111-4111-8111-111111111111', 19.2, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PRIMOR PASTA CORTA DEDALES 1KGx12UN', 24.0, 2.0, 'ALI-PRI-0052', 48, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000053', 'Primor', 'a1111111-1111-4111-8111-111111111111', 19.2, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PRIMOR PASTA CORTA TORNILLO 1KGx12UN', 24.0, 2.0, 'ALI-PRI-0053', 132, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000054', 'Primor', 'a1111111-1111-4111-8111-111111111111', 19.2, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PRIMOR PASTA CORTA PLUMITAS 1KGx12UN', 24.0, 2.0, 'ALI-PRI-0054', 216, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000055', 'Primor', 'a1111111-1111-4111-8111-111111111111', 9.7, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PRIMOR PASTA CORTA DEDALES 500Gx12UN', 12.12, 1.01, 'ALI-PRI-0055', 300, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000056', 'Primor', 'a1111111-1111-4111-8111-111111111111', 9.7, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PRIMOR PASTA CORTA PLUMITAS 500Gx12UN', 12.12, 1.01, 'ALI-PRI-0056', 384, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000057', 'Primor', 'a1111111-1111-4111-8111-111111111111', 8.83, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PRIMOR PASTA LARGA VERMICELL 500Gx12UN', 11.04, 0.92, 'ALI-PRI-0057', 468, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000058', 'Primor', 'a1111111-1111-4111-8111-111111111111', 17.57, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PRIMOR PASTA L VERMICELLI 1KGx12UN NR', 21.96, 1.83, 'ALI-PRI-0058', 12, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000059', 'Primor', 'a1111111-1111-4111-8111-111111111111', 15.94, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PRIMOR EXTRA ESPECIAL VERMICELLI 1KGx12U', 19.92, 1.66, 'ALI-PRI-0059', 96, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000060', 'Primor', 'a1111111-1111-4111-8111-111111111111', 17.57, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PRIMOR EXTRA ESPECIAL PLUMITAS 1KGx12U', 21.96, 1.83, 'ALI-PRI-0060', 180, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000061', 'Primor', 'a1111111-1111-4111-8111-111111111111', 17.57, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PRIMOR EXTRA ESPECIAL TORNILLOS 1KGx12U', 21.96, 1.83, 'ALI-PRI-0061', 264, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000062', 'Primor', 'a1111111-1111-4111-8111-111111111111', 17.57, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PRIMOR EXTRA ESPECIAL DEDALES 1KGx12U', 21.96, 1.83, 'ALI-PRI-0062', 348, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000063', 'Ketchup', 'a1111111-1111-4111-8111-111111111111', 25.83, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'KETCHUP PAMPERO 198 G X 24 UND', 32.29, 1.35, 'ALI-KET-0063', 864, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000064', 'Pampero', 'a1111111-1111-4111-8111-111111111111', 38.98, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'PAMPERO KETCHUP 397Gx24UN', 48.72, 2.03, 'ALI-PAM-0064', 1032, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000065', 'Pampero', 'a1111111-1111-4111-8111-111111111111', 50.41, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 160, 20, 'PAMPERO SALSA BASE TOMATE 4,2KGx4UN CFH', 63.01, 15.75, 'ALI-PAM-0065', 20, 4);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000066', 'Mavesa', 'a1111111-1111-4111-8111-111111111111', 20.05, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'MAVESA VINAGRE DE ALCOHOL 1Lx12UN', 25.06, 2.09, 'ALI-MAV-0066', 144, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000067', 'Mavesa', 'a1111111-1111-4111-8111-111111111111', 23.83, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'MAVESA VINAGRE 500MLx24UN', 29.79, 1.24, 'ALI-MAV-0067', 456, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000068', 'Mavesa', 'a1111111-1111-4111-8111-111111111111', 23.61, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 160, 20, 'MAVESA VINAGRE DE ALCOHOL 4Lx4UN CFH', 29.51, 7.38, 'ALI-MAV-0068', 104, 4);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000069', 'Mavesa', 'a1111111-1111-4111-8111-111111111111', 24.24, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'MAVESA MARGARINA 1000Gx6UN', 30.3, 5.05, 'ALI-MAV-0069', 198, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000070', 'Mavesa', 'a1111111-1111-4111-8111-111111111111', 25.82, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'MAVESA MARGARINA 500Gx12UN', 32.28, 2.69, 'ALI-MAV-0070', 480, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000071', 'Mavesa', 'a1111111-1111-4111-8111-111111111111', 28.8, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'MAVESA MARGARINA 250Gx24UN', 36.0, 1.5, 'ALI-MAV-0071', 48, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000072', 'Nelly', 'a1111111-1111-4111-8111-111111111111', 24.58, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'NELLY MARGARINA REDUCIDA CAL 250Gx24UN', 30.72, 1.28, 'ALI-NEL-0072', 216, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000073', 'Nelly', 'a1111111-1111-4111-8111-111111111111', 23.9, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'NELLY MARGARINA REDUCIDA CAL 500Gx12UN', 29.88, 2.49, 'ALI-NEL-0073', 192, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000074', 'Mavesa', 'a1111111-1111-4111-8111-111111111111', 35.23, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'MAVESA MAYONESA 910Gx6UN', 44.04, 7.34, 'ALI-MAV-0074', 138, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000075', 'Mavesa', 'a1111111-1111-4111-8111-111111111111', 37.15, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'MAVESA MAYONESA 445Gx12UN', 46.44, 3.87, 'ALI-MAV-0075', 360, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000076', 'Mavesa', 'a1111111-1111-4111-8111-111111111111', 37.25, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'MAVESA MAYONESA 175Gx24UN STOCK', 46.56, 1.94, 'ALI-MAV-0076', 888, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000077', 'Mavesa', 'a1111111-1111-4111-8111-111111111111', 92.54, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 160, 20, 'MAVESA ADEREZO MAYONESA 3,6KGx4UN CFH', 115.68, 28.92, 'ALI-MAV-0077', 176, 4);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000078', 'Café Anzotegui', 'a1111111-1111-4111-8111-111111111111', 29.76, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 1200, 150, 'CAFÉ ANZOTEGUI 100X30UN', 37.2, 1.24, 'ALI-CAF-0078', 180, 30);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000079', 'Café Anzotegui', 'a1111111-1111-4111-8111-111111111111', 38.72, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 800, 100, 'CAFÉ ANZOTEGUI 200X20UN', 48.4, 2.42, 'ALI-CAF-0079', 260, 20);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000080', 'Café Anzotegui', 'a1111111-1111-4111-8111-111111111111', 47.84, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 400, 50, 'CAFÉ ANZOTEGUI 500X10UN', 59.8, 5.98, 'ALI-CAF-0080', 200, 10);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000081', 'Café Anzoategui', 'a1111111-1111-4111-8111-111111111111', 26.4, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 2000, 250, 'CAFÉ ANZOATEGUI 50X50UN', 33.0, 0.66, 'ALI-CAF-0081', 1350, 50);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000082', 'Buen Café', 'a1111111-1111-4111-8111-111111111111', 30.0, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 1200, 150, 'BUEN CAFÉ 100X30UN', 37.5, 1.25, 'ALI-BUE-0082', 1020, 30);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000083', 'Buen Café', 'a1111111-1111-4111-8111-111111111111', 39.36, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 800, 100, 'BUEN CAFÉ 200X20UN', 49.2, 2.46, 'ALI-BUE-0083', 820, 20);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000084', 'Buen Café', 'a1111111-1111-4111-8111-111111111111', 48.88, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 400, 50, 'BUEN CAFÉ 500X10UN', 61.1, 6.11, 'ALI-BUE-0084', 30, 10);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000085', 'Margarita', 'a1111111-1111-4111-8111-111111111111', 63.0, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 1400, 175, 'MARGARITA ATUN NATURAL 140Gx35UN', 78.75, 2.25, 'ALI-MAR-0085', 350, 35);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000086', 'Margarita', 'a1111111-1111-4111-8111-111111111111', 50.88, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'MARGARITA ATUN EN AGUA 170Gx24UN', 63.6, 2.65, 'ALI-MAR-0086', 408, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000087', 'Margarita', 'a1111111-1111-4111-8111-111111111111', 69.83, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 1400, 175, 'MARGARITA ATUN EN ACEITE 140Gx35UN', 87.29, 2.49, 'ALI-MAR-0087', 840, 35);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000088', 'Margarita', 'a1111111-1111-4111-8111-111111111111', 56.79, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'MARGARITA ATUN EN ACEITE 170Gx24UN', 70.99, 2.96, 'ALI-MAR-0088', 744, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000089', 'Margarita', 'a1111111-1111-4111-8111-111111111111', 59.02, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'MARGARITA ATUN TROZOS ACEITE 170Gx24UN', 73.78, 3.07, 'ALI-MAR-0089', 912, 24);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000090', 'California', 'a1111111-1111-4111-8111-111111111111', 47.42, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 1400, 175, 'CALIFORNIA ATUN DESMENUZADO 140Gx35UN', 59.28, 1.69, 'ALI-CAL-0090', 35);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000091', 'Margarita', 'a1111111-1111-4111-8111-111111111111', 48.72, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 1400, 175, 'MARGARITA PEPITONA PIC. 140Gx35UN', 60.9, 1.74, 'ALI-MAR-0091', 245, 35);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000092', 'Margarita', 'a1111111-1111-4111-8111-111111111111', 17.76, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 800, 100, 'MARGARITA SARDINA ACEITE 170Gx20UN', 22.2, 1.11, 'ALI-MAR-0092', 280, 20);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000093', 'Margarita', 'a1111111-1111-4111-8111-111111111111', 17.76, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 800, 100, 'MARGARITA SARDINA PIC. 170Gx20UN', 22.2, 1.11, 'ALI-MAR-0093', 420, 20);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000094', 'Margarita', 'a1111111-1111-4111-8111-111111111111', 17.76, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 800, 100, 'MARGARITA SARDINA TOMATE 170Gx20UN', 22.2, 1.11, 'ALI-MAR-0094', 560, 20);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000095', 'Margarita', 'a1111111-1111-4111-8111-111111111111', 17.76, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 800, 100, 'MARGARITA SARDINA AHUMADA 170Gx20UN', 22.2, 1.11, 'ALI-MAR-0095', 700, 20);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000096', 'Primor', 'a1111111-1111-4111-8111-111111111111', 30.91, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'PRIMOR CREMA ARROZ ENRIQUECI 450Gx24UN', 38.64, 1.61, 'ALI-PRI-0096', 1008, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000097', 'Primor', 'a1111111-1111-4111-8111-111111111111', 24.58, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PRIMOR CREMA ARROZ ENRIQUECI 900Gx12UN', 30.72, 2.56, 'ALI-PRI-0097', 48, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000098', 'Primor', 'a1111111-1111-4111-8111-111111111111', 11.36, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 800, 100, 'PRIMOR CREMA ARROZ BOLSA ENRIQ 225Gx20UN', 14.2, 0.71, 'ALI-PRI-0098', 220, 20);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000099', 'Primor', 'a1111111-1111-4111-8111-111111111111', 15.87, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 640, 80, 'PRIMOR CREMA ARROZ BOLSA ENRIQ 450Gx16UN', 19.84, 1.24, 'ALI-PRI-0099', 288, 16);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000100', 'Primor', 'a1111111-1111-4111-8111-111111111111', 18.91, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PRIMOR CREMA ARROZ BOLSA ENRIQ 900Gx12UN', 23.64, 1.97, 'ALI-PRI-0100', 300, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000101', 'Quaker', 'a1111111-1111-4111-8111-111111111111', 19.2, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 640, 80, 'QUAKER AVENA ORIGINAL BOLSA 400Gx16UN', 24.0, 1.5, 'ALI-QUA-0101', 512, 16);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000102', 'Quaker', 'a1111111-1111-4111-8111-111111111111', 25.92, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'QUAKER AVENA ORIGINAL BOLSA 800Gx12UN', 32.4, 2.7, 'ALI-QUA-0102', 468, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000103', 'Quaker', 'a1111111-1111-4111-8111-111111111111', 19.2, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 640, 80, 'QUAKER HARINA DE AVENA (BOLS) 400Gx16UN', 24.0, 1.5, 'ALI-QUA-0103', 16, 16);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000104', 'Quaker', 'a1111111-1111-4111-8111-111111111111', 12.16, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 800, 100, 'QUAKER AVENA ORIGINAL BOLSA 200Gx20UN', 15.2, 0.76, 'ALI-QUA-0104', 160, 20);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000105', 'Toddy', 'a1111111-1111-4111-8111-111111111111', 105.34, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'TODDY BOLSA 1KGx12UN', 131.68, 10.97, 'ALI-TOD-0105', 180, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000106', 'Toddy', 'a1111111-1111-4111-8111-111111111111', 131.7, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 320, 40, 'TODDY 2KGx8UN', 164.63, 20.58, 'ALI-TOD-0106', 176, 8);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000107', 'Toddy', 'a1111111-1111-4111-8111-111111111111', 95.54, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'TODDY BOLSA 400Gx24UN', 119.43, 4.98, 'ALI-TOD-0107', 696, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000108', 'Toddy', 'a1111111-1111-4111-8111-111111111111', 47.78, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'TODDY LATA 400Gx12UN', 59.72, 4.98, 'ALI-TOD-0108', 432, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000109', 'Toddy', 'a1111111-1111-4111-8111-111111111111', 25.28, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'TODDY ENVASE 200Gx12UN', 31.6, 2.63, 'ALI-TOD-0109', 516, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000110', 'Toddy', 'a1111111-1111-4111-8111-111111111111', 40.76, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 1440, 180, 'TODDY BOLSA 100GX36UN', 50.95, 1.42, 'ALI-TOD-0110', 180, 36);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000111', 'Rikesa', 'a1111111-1111-4111-8111-111111111111', 45.6, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 720, 90, 'RIKESA QUESO CHEDDAR PICANTE 200Gx18UN', 57.0, 3.17, 'ALI-RIK-0111', 216, 18);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000112', 'Rikesa', 'a1111111-1111-4111-8111-111111111111', 75.5, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 720, 90, 'RIKESA QUESO CHEDDAR SQUEEZE 330Gx18UN', 94.38, 5.24, 'ALI-RIK-0112', 342, 18);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000113', 'Rikesa', 'a1111111-1111-4111-8111-111111111111', 49.94, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 720, 90, 'RIKESA QUESO ORIGINAL ENRIQ 200Gx18UN', 62.43, 3.47, 'ALI-RIK-0113', 468, 18);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000114', 'Rikesa', 'a1111111-1111-4111-8111-111111111111', 48.22, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'RIKESA QUESO ORIGINAL ENRIQ 300Gx12UN', 60.27, 5.02, 'ALI-RIK-0114', 396, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000115', 'Rikesa', 'a1111111-1111-4111-8111-111111111111', 48.22, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'RIKESA QUESO TOCINETA ENRIQ 300Gx12UN', 60.27, 5.02, 'ALI-RIK-0115', 480, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000116', 'Konga', 'a1111111-1111-4111-8111-111111111111', 37.42, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 4480, 560, 'KONGA SABOR LIMON 30GX8X14UN', 46.77, 0.42, 'ALI-KON-0116', 224, 112);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000117', 'Konga', 'a1111111-1111-4111-8111-111111111111', 37.42, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 4480, 560, 'KONGA SABOR NARANJA 30GX8X14UN', 46.77, 0.42, 'ALI-KON-0117', 1008, 112);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000118', 'Konga', 'a1111111-1111-4111-8111-111111111111', 37.42, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 4480, 560, 'KONGA SABOR A MORA 30GX8X14UN', 46.77, 0.42, 'ALI-KON-0118', 1792, 112);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000119', 'Konga', 'a1111111-1111-4111-8111-111111111111', 37.42, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 4480, 560, 'KONGA SABOR PARCHITA 30GX8X14UN', 46.77, 0.42, 'ALI-KON-0119', 2576, 112);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000120', 'Konga', 'a1111111-1111-4111-8111-111111111111', 37.42, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 4480, 560, 'KONGA SABOR TIZANA 30GX8X14UN', 46.77, 0.42, 'ALI-KON-0120', 3360, 112);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000121', 'Konga', 'a1111111-1111-4111-8111-111111111111', 37.42, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 4480, 560, 'KONGA SABOR DURAZNO 30GX8X14UN', 46.77, 0.42, 'ALI-KON-0121', 4144, 112);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000122', 'Konga', 'a1111111-1111-4111-8111-111111111111', 37.42, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 4480, 560, 'KONGA PAPELON CON LIMON 30GX8X14UNI', 46.77, 0.42, 'ALI-KON-0122', 4928, 112);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000123', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 32.26, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 1920, 240, 'LAS LLAVES JABON FF BEBE 160X48UN', 40.32, 0.84, 'ALI-LAS-0123', 288, 48);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000124', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 30.62, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 1760, 220, 'LAS LLAVES JABON TRAD. FLORAL 200Gx44UN', 38.28, 0.87, 'ALI-LAS-0124', 572, 44);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000125', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 30.82, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 1440, 180, 'LAS LLAVES JABON TRAD. FLORAL 250Gx36UN', 38.52, 1.07, 'ALI-LAS-0125', 720, 36);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000126', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 17.79, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 640, 80, 'LAS LLAVES DETERGENTE LIMON 400Gx16UN', 22.24, 1.39, 'ALI-LAS-0126', 432, 16);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000127', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 24.32, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 400, 50, 'LAS LLAVES DETERGENTE P. LI 900Gx10UN', 30.4, 3.04, 'ALI-LAS-0127', 340, 10);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000128', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 17.79, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 640, 80, 'LAS LLAVES DETERGENTE FLORAL 400Gx16UN', 22.24, 1.39, 'ALI-LAS-0128', 656, 16);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000129', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 24.32, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 400, 50, 'LAS LLAVES DETERGENTE FLORAL 900Gx10UN', 30.4, 3.04, 'ALI-LAS-0129', 30, 10);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000130', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 24.32, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 400, 50, 'LAS LLAVES DETERGENTE BEBE 900Gx10UN', 30.4, 3.04, 'ALI-LAS-0130', 100, 10);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000131', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 17.79, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 640, 80, 'LAS LLAVES DETERGENTE BEBE 400Gx16UN', 22.24, 1.39, 'ALI-LAS-0131', 272, 16);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000132', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 13.95, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 640, 80, 'LAS LLAVES LINEA ACTIVA CITRICA 400GRX16UN', 17.44, 1.09, 'ALI-LAS-0132', 384, 16);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000133', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 13.95, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 640, 80, 'LAS LLAVES LINEA ACTIVA FLORAL 400GRX16UN', 17.44, 1.09, 'ALI-LAS-0133', 496, 16);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000134', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 18.64, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 400, 50, 'LAS LLAVES LINEA ACTIVA FLORAL 900GRX10UN', 23.3, 2.33, 'ALI-LAS-0134', 380, 10);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000135', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 18.64, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 400, 50, 'LAS LLAVES LINEA ACTIVA CITRICO 900GRX10UN', 23.3, 2.33, 'ALI-LAS-0135', 10);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000136', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 69.41, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 1440, 180, 'LAS LLAVES MULTIUSO CREMA 250Gx36UN', 86.76, 2.41, 'ALI-LAS-0136', 252, 36);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000137', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 62.47, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 720, 90, 'LAS LLAVES MULTIUSO CREMA 500Gx18UN', 78.09, 4.34, 'ALI-LAS-0137', 252, 18);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000138', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 58.65, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 640, 80, 'LAS LLAVES LAV. L. ANTIBACT 500CCGx16UN', 73.31, 4.58, 'ALI-LAS-0138', 336, 16);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000139', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 42.54, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'LAS LLAVES LIMP MAREA CRISTALINA 1LX12UN', 53.17, 4.43, 'ALI-LAS-0139', 336, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000140', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 42.54, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'LAS LLAVES LIMP BRISA TROPICAL 1LX12UN', 53.17, 4.43, 'ALI-LAS-0140', 420, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000141', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 48.1, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'LAS LLAVES LIMP. MAREA CRISTA 500CCx24UN', 60.13, 2.51, 'ALI-LAS-0141', 1008, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000142', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 48.1, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'LAS LLAVES LIMP BOSQUE SERENO 500CCx24UN', 60.13, 2.51, 'ALI-LAS-0142', 96, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000143', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 42.54, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'LAS LLAVES LIMP BOSQUE SERENO 1Lx12UN', 53.17, 4.43, 'ALI-LAS-0143', 132, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000144', 'Las Llaves', 'a1111111-1111-4111-8111-111111111111', 42.54, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'LAS LLAVES LIMP FRESCURA R. 1LX12UN', 53.17, 4.43, 'ALI-LAS-0144', 216, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000145', 'Multi Clean', 'a1111111-1111-4111-8111-111111111111', 37.3, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 800, 100, 'MULTI CLEAN DETER FRAG CITRICA 900Gx20UN', 46.63, 2.33, 'ALI-MUL-0145', 500, 20);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000146', 'Multi Clean', 'a1111111-1111-4111-8111-111111111111', 26.17, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 1200, 150, 'MULTI CLEAN DETER FRAG CITRICA 400Gx30UN', 32.71, 1.09, 'ALI-MUL-0146', 960, 30);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000147', 'Multi Clean', 'a1111111-1111-4111-8111-111111111111', 41.5, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 160, 20, 'MULTI CLEAN DETER FRAG CITRICA 5KGx4UN', 51.88, 12.97, 'ALI-MUL-0147', 156, 4);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000148', 'Multi Clean', 'a1111111-1111-4111-8111-111111111111', 37.3, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 800, 100, 'MULTI CLEAN DETER FRAG FLORAL 900Gx20UN', 46.63, 2.33, 'ALI-MUL-0148', 20, 20);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000149', 'Multi Clean', 'a1111111-1111-4111-8111-111111111111', 26.17, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 1200, 150, 'MULTI CLEAN DETER FRAG FLORAL 400Gx30UN', 32.71, 1.09, 'ALI-MUL-0149', 240, 30);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000150', 'Multi Clean', 'a1111111-1111-4111-8111-111111111111', 41.5, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 160, 20, 'MULTI CLEAN DETER FRAG FLORAL 5KGx4UN', 51.88, 12.97, 'ALI-MUL-0150', 60, 4);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000151', 'Super Can', 'a1111111-1111-4111-8111-111111111111', 37.53, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'SUPER CAN CACHORRO 2KGx6UN', 46.91, 7.82, 'ALI-SUP-0151', 132, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000152', 'Super Can', 'a1111111-1111-4111-8111-111111111111', 38.16, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 40, 5, 'SUPER CAN CACHORRO 18KGx1UN', 47.7, 47.7, 'ALI-SUP-0152', 29, 1);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000153', 'Super Can', 'a1111111-1111-4111-8111-111111111111', 33.74, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 40, 5, 'SUPER CAN CARNE HUESO 18KILOS', 42.18, 42.18, 'ALI-SUP-0153', 36, 1);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000154', 'Super Can', 'a1111111-1111-4111-8111-111111111111', 33.74, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 40, 5, 'SUPER CAN POLLO 18KILOS', 42.18, 42.18, 'ALI-SUP-0154', 43, 1);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000155', 'Super Can', 'a1111111-1111-4111-8111-111111111111', 49.32, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 200, 25, 'SUPER CAN CARNE HUESO 4KGx5UN', 61.65, 12.33, 'ALI-SUP-0155', 25, 5);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000156', 'Super Can', 'a1111111-1111-4111-8111-111111111111', 55.08, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 200, 25, 'SUPER CAN CACHORROS 4KGx5UN', 68.85, 13.77, 'ALI-SUP-0156', 60, 5);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000157', 'Super Can', 'a1111111-1111-4111-8111-111111111111', 30.06, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'SUPER CAN CARNE HUESO 2KGx6UN', 37.58, 6.26, 'ALI-SUP-0157', 114, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000158', 'Super Can', 'a1111111-1111-4111-8111-111111111111', 22.33, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 40, 5, 'SUPER CAN CARNE HUESO 10KGx1UN', 27.91, 27.91, 'ALI-SUP-0158', 26, 1);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000159', 'Champs', 'a1111111-1111-4111-8111-111111111111', 27.84, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 40, 5, 'CHAMPS PERRO CARNE TACO 20KGx1UN', 34.8, 34.8, 'ALI-CHA-0159', 33, 1);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000160', 'Dogourmet', 'a1111111-1111-4111-8111-111111111111', 44.94, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 40, 5, 'DOGOURMET CARNE A LA PARRILLA 18KGx1UN', 56.18, 56.18, 'ALI-DOG-0160', 40, 1);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000161', 'Dogourmet', 'a1111111-1111-4111-8111-111111111111', 41.42, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'DOGOURMET CARNE A LA PARRILLA 2KGx6UN', 51.78, 8.63, 'ALI-DOG-0161', 12, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000162', 'Dogourmet', 'a1111111-1111-4111-8111-111111111111', 44.94, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 40, 5, 'DOGOURMET POLLO A LA BRASA 18KGx1UN', 56.18, 56.18, 'ALI-DOG-0162', 9, 1);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000163', 'Dogourmet', 'a1111111-1111-4111-8111-111111111111', 50.6, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 40, 5, 'DOGOURMET CACHORROS 18KGx1UN', 63.25, 63.25, 'ALI-DOG-0163', 16, 1);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000164', 'Dogourmet', 'a1111111-1111-4111-8111-111111111111', 45.38, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'DOGOURMET CACHORROS 2KGx6UN', 56.72, 9.45, 'ALI-DOG-0164', 138, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000165', 'Dogourmet', 'a1111111-1111-4111-8111-111111111111', 64.73, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 200, 25, 'DOGOURMET CARNE A LA PARRILLA 4KGx5UN', 80.91, 16.18, 'ALI-DOG-0165', 150, 5);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000166', 'Dogourmet', 'a1111111-1111-4111-8111-111111111111', 64.73, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 200, 25, 'DOGOURMET POLLO A LA BRASA 4KGx5UN', 80.91, 16.18, 'ALI-DOG-0166', 185, 5);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000167', 'Dogourmet', 'a1111111-1111-4111-8111-111111111111', 36.29, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 400, 50, 'DOGOURMET CARNE A LA PARRILLA 1KGx10UN', 45.36, 4.54, 'ALI-DOG-0167', 440, 10);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000168', 'Dogourmet', 'a1111111-1111-4111-8111-111111111111', 44.94, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 40, 5, 'DOGOURMET ASADO NEGRO 18KGx1UN', 56.18, 56.18, 'ALI-DOG-0168', 6, 1);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000169', 'Dogourmet', 'a1111111-1111-4111-8111-111111111111', 64.73, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 200, 25, 'DOGOURMET ASADO NEGRO 4KGx5UN', 80.91, 16.18, 'ALI-DOG-0169', 65, 5);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000170', 'Dogourmet', 'a1111111-1111-4111-8111-111111111111', 47.25, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 40, 5, 'DOGOURMET PARRILLA MIXTA 18KGx1UN', 59.06, 59.06, 'ALI-DOG-0170', 20, 1);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000171', 'Dogourmet', 'a1111111-1111-4111-8111-111111111111', 67.88, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 200, 25, 'DOGOURMET PARRILLA MIXTA 4KGx5UN', 84.85, 16.97, 'ALI-DOG-0171', 135, 5);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000172', 'Dogourmet', 'a1111111-1111-4111-8111-111111111111', 30.93, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 40, 5, 'DOGOURMET CACHORROS 10KGx1UN', 38.66, 38.66, 'ALI-DOG-0172', 34, 1);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000173', 'Dogourmet', 'a1111111-1111-4111-8111-111111111111', 28.14, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 40, 5, 'DOGOURMET CARNE A LA PARRILLA 10KGx1UN', 35.17, 35.17, 'ALI-DOG-0173', 41, 1);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000174', 'Ohmaigat', 'a1111111-1111-4111-8111-111111111111', 62.42, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'OHMAIGAT CASEROS Y DELICADOS 1,5KGX6UND', 78.02, 13.0, 'ALI-OHM-0174', 18, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000175', 'Ohmaigat', 'a1111111-1111-4111-8111-111111111111', 43.66, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'OHMAIGAT CASEROS Y DELICADOS 500Gx12UND', 54.57, 4.55, 'ALI-OHM-0175', 120, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000176', 'Donkat', 'a1111111-1111-4111-8111-111111111111', 89.54, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 720, 90, 'DONKAT GATICOS 1KGX18UND', 111.92, 6.22, 'ALI-DON-0176', 306, 18);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000177', 'Donkat', 'a1111111-1111-4111-8111-111111111111', 78.09, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 600, 75, 'DONKAT ADULTO 1,1KGX15UND', 97.61, 6.51, 'ALI-DON-0177', 360, 15);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000178', 'Donkat', 'a1111111-1111-4111-8111-111111111111', 22.46, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 40, 5, 'DONKAT ADULTO 7KGX1UND', 28.08, 28.08, 'ALI-DON-0178', 31, 1);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000179', 'Donkat', 'a1111111-1111-4111-8111-111111111111', 54.18, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 40, 5, 'DONKAT ADULTO 16KGx1UND', 67.72, 67.72, 'ALI-DON-0179', 38, 1);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000180', 'Donkat', 'a1111111-1111-4111-8111-111111111111', 23.35, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 40, 5, 'DONKAT GATICOS 7KGX1UND', 29.19, 29.19, 'ALI-DON-0180', 1);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000181', 'Gillette', 'c3333333-3333-4333-8333-333333333333', 93.32, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'GILLETTE BOLSA PB2X5UN MASC 39GX24UN', 116.65, 4.86, 'JAB-GIL-0181', 168, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000182', 'Gillette', 'c3333333-3333-4333-8333-333333333333', 448.56, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'GILLETTE RISTRA PB2X24UN MASC 168GX24RIS', 560.7, 23.36, 'JAB-GIL-0182', 336, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000183', 'Gillette', 'c3333333-3333-4333-8333-333333333333', 12.46, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 400, 50, 'GILLETTE RISTRA PB3X10UN MASC 120GX12RIS', 15.57, 1.55, 'JAB-GIL-0183', 210, 10);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000184', 'Gillette', 'c3333333-3333-4333-8333-333333333333', 227.84, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 2880, 360, 'GILLETTE BLIST PB3X2 SENSECARE 23GX72UN', 284.8, 3.96, 'JAB-GIL-0184', 2016, 72);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000185', 'Gillette', 'c3333333-3333-4333-8333-333333333333', 229.18, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 2880, 360, 'GILLETTE BLISTER CUERPOX2 23GX72UND', 286.47, 3.98, 'JAB-GIL-0185', 2520, 72);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000186', 'Venus', 'c3333333-3333-4333-8333-333333333333', 99.58, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 400, 50, 'VENUS RISTRA SIMPLYX8UN FEM 104GX10RIS', 124.47, 12.45, 'JAB-VEN-0186', 420, 10);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000187', 'Oralb', 'c3333333-3333-4333-8333-333333333333', 65.76, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'ORALB CREMADENT 100% 6X50ML X4UN', 82.2, 13.7, 'JAB-ORA-0187', 24, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000188', 'Oralb', 'c3333333-3333-4333-8333-333333333333', 26.0, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'ORALB CEP 1-2-3 PAQX6UN 120GX6RIS', 32.5, 5.42, 'JAB-ORA-0188', 66, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000189', 'Ace', 'c3333333-3333-4333-8333-333333333333', 36.33, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 600, 75, 'ACE DETERGENTE POLVO COL 800GX15UN', 45.41, 3.03, 'JAB-ACE-0189', 270, 15);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000190', 'Ace', 'c3333333-3333-4333-8333-333333333333', 30.06, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'ACE DETERGENTE POLVO COL 400GX24UN', 37.58, 1.57, 'JAB-ACE-0190', 600, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000191', 'Ariel', 'c3333333-3333-4333-8333-333333333333', 43.98, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 600, 75, 'ARIEL DETER TOUCH OF DOWNY COL 800GX15UN', 54.98, 3.67, 'JAB-ARI-0191', 480, 15);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000192', 'Ariel', 'c3333333-3333-4333-8333-333333333333', 51.56, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 160, 20, 'ARIEL DETERGENTE POLVO COL 4KGX4UN', 64.45, 16.11, 'JAB-ARI-0192', 156, 4);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000193', 'Ariel', 'c3333333-3333-4333-8333-333333333333', 43.98, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 600, 75, 'ARIEL DETERGENTE POLVO COL 800GX15UN', 54.98, 3.67, 'JAB-ARI-0193', 15, 15);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000194', 'Ariel', 'c3333333-3333-4333-8333-333333333333', 36.08, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'ARIEL DETERGENTE POLVO COL 400GX24UN', 45.1, 1.88, 'JAB-ARI-0194', 192, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000195', 'Downy', 'c3333333-3333-4333-8333-333333333333', 56.02, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 360, 45, 'DOWNY SUAVIZANTE FLORAL 1400CCX9UN', 70.02, 7.78, 'JAB-DOW-0195', 135, 9);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000196', 'Downy', 'c3333333-3333-4333-8333-333333333333', 39.46, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'DOWNY SUAVIZANTE AMANECER 700CCX12UN', 49.32, 4.11, 'JAB-DOW-0196', 264, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000197', 'Downy', 'c3333333-3333-4333-8333-333333333333', 39.46, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'DOWNY SUAVIZANTE FLORAL 700CCX12UN', 49.32, 4.11, 'JAB-DOW-0197', 348, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000198', 'Downy', 'c3333333-3333-4333-8333-333333333333', 24.86, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'DOWNY SUAVIZANTE FLORAL 360CCX12UN', 31.08, 2.59, 'JAB-DOW-0198', 432, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000199', 'Downy', 'c3333333-3333-4333-8333-333333333333', 24.86, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'DOWNY SUAVIZANTE AMANECER 360CCX12UN', 31.08, 2.59, 'JAB-DOW-0199', 516, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000200', 'Downy', 'c3333333-3333-4333-8333-333333333333', 39.46, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'DOWNY SUAVIZANTE EXOTICO 700CCX12UN', 49.32, 4.11, 'JAB-DOW-0200', 60, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000201', 'Downy', 'c3333333-3333-4333-8333-333333333333', 53.37, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 360, 45, 'DOWNY SUAVIZANTE EXOTICO 1400CCX9UN', 66.71, 7.41, 'JAB-DOW-0201', 108, 9);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000202', 'Downy', 'c3333333-3333-4333-8333-333333333333', 23.61, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'DOWNY SUAVIZANTE EXOTICO 360CCX12UN', 29.51, 2.46, 'JAB-DOW-0202', 228, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000203', 'Pampers', 'c3333333-3333-4333-8333-333333333333', 38.98, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 320, 40, 'PAMPERS PAÑALES CONFORT SEC XXG 432GX8UN', 48.72, 6.09, 'JAB-PAM-0203', 208, 8);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000204', 'Pampers', 'c3333333-3333-4333-8333-333333333333', 38.98, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 320, 40, 'PAMPERS PAÑALES CONFORT SEC XG 451GX8UN', 48.72, 6.09, 'JAB-PAM-0204', 264, 8);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000205', 'Pampers', 'c3333333-3333-4333-8333-333333333333', 38.98, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 320, 40, 'PAMPERS PAÑALES CONFORT SEC G 463GX8UN', 48.72, 6.09, 'JAB-PAM-0205', 320, 8);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000206', 'Pampers', 'c3333333-3333-4333-8333-333333333333', 58.46, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PAMPERS PAÑALES CONFORT SEC M 491GX12UN', 73.08, 6.09, 'JAB-PAM-0206', 24, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000207', 'Pampers', 'c3333333-3333-4333-8333-333333333333', 19.49, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 160, 20, 'PAMPERS PAÑALES CONFORT SEC P 432GX4UN', 24.36, 6.09, 'JAB-PAM-0207', 36, 4);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000208', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 78.51, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN ACOND RESTAURACION 400CCX12UN', 98.14, 8.18, 'JAB-PTN-0208', 192, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000209', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 47.22, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN ACOND RESTAURACION 200CCX12UN', 59.02, 4.92, 'JAB-PTN-0209', 276, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000210', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 86.08, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN ACOND 3MM RESTAURACION 170CCX12UN', 107.6, 8.97, 'JAB-PTN-0210', 360, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000211', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 86.08, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN ACOND 3MM LISO EXT 170CCX12UN', 107.6, 8.97, 'JAB-PTN-0211', 444, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000212', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 86.08, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN ACOND 3MM HIDRAT EXTR 170CCX12UN', 107.6, 8.97, 'JAB-PTN-0212', 528, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000213', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 47.22, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN SHAMPOO BAMBU 200CCX12UN', 59.02, 4.92, 'JAB-PTN-0213', 72, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000214', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 78.51, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN SHAMPOO BAMBU 400CCX12UN', 98.14, 8.18, 'JAB-PTN-0214', 156, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000215', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 78.51, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN ACOND BAMBU 400CCX12UN', 98.14, 8.18, 'JAB-PTN-0215', 240, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000216', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 47.22, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN ACOND BAMBU 200CCX12UN', 59.02, 4.92, 'JAB-PTN-0216', 324, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000217', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 87.19, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN INTENSIVE TREATMENT 300CCX12UN', 108.99, 9.08, 'JAB-PTN-0217', 408, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000218', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 87.19, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN TRATAMIENTO HIDRATACIÓN 300CCX12UN', 108.99, 9.08, 'JAB-PTN-0218', 492, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000219', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 87.19, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN TRATAMIENTO NUTRICIÓN 300CCX12UN', 108.99, 9.08, 'JAB-PTN-0219', 36, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000220', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 78.51, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN SHAMPOO COLAGENO 300CCX12UN', 98.14, 8.18, 'JAB-PTN-0220', 120, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000221', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 125.84, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN SHAMPOO COLAGENO 510CCX12UN', 157.3, 13.11, 'JAB-PTN-0221', 204, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000222', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 125.84, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN ACOND COLAGENO 510CCX12UN', 157.3, 13.11, 'JAB-PTN-0222', 288, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000223', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 75.06, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN CREMA PEINAR COLAGENO 300CCX12UN', 93.82, 7.82, 'JAB-PTN-0223', 372, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000224', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 78.51, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN ACOND COLAGENO 250CCX12UN', 98.14, 8.18, 'JAB-PTN-0224', 456, 12);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000225', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 75.06, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN CREMA PEINAR RESTAURACION 300CCX12UN', 93.82, 7.82, 'JAB-PTN-0225', 12);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000226', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 75.06, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN CREMA PEINAR LISO EXT 300CCX12UN', 93.82, 7.82, 'JAB-PTN-0226', 84, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000227', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 47.23, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN SHAMPOO RESTAURACION 200CCX12UN', 59.04, 4.92, 'JAB-PTN-0227', 168, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000228', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 78.51, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN SHAMPOO RESTAURACION 400CCX12UN', 98.14, 8.18, 'JAB-PTN-0228', 252, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000229', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 125.84, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN ACOND KERATINA 510CCX12UN', 157.3, 13.11, 'JAB-PTN-0229', 336, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000230', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 125.84, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN SHAMPOO KERATINA 510CCX12UN', 157.3, 13.11, 'JAB-PTN-0230', 420, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000231', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 75.06, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN CREMA PEINAR KERATINA 300CCX12UN', 93.82, 7.82, 'JAB-PTN-0231', 504, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000232', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 87.19, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN ACOND KERATINA 250CCX12UN', 108.99, 9.08, 'JAB-PTN-0232', 48, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000233', 'Ptn', 'c3333333-3333-4333-8333-333333333333', 87.19, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN SHAMPOO KERATINA 300CCX12UN', 108.99, 9.08, 'JAB-PTN-0233', 132, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000234', 'Hs', 'c3333333-3333-4333-8333-333333333333', 82.18, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'HS ACOND ANTIFALL 300CCX12UN', 102.73, 8.56, 'JAB-HSA-0234', 216, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000235', 'Hs', 'c3333333-3333-4333-8333-333333333333', 82.18, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'HS SHAMPOO 2en1 LIMP RENOV 375CCX12UN', 102.73, 8.56, 'JAB-HSS-0235', 300, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000236', 'Hs', 'c3333333-3333-4333-8333-333333333333', 82.18, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'HS SHAMPOO ANTIFALL 375CCX12UN', 102.73, 8.56, 'JAB-HSS-0236', 384, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000237', 'Hs', 'c3333333-3333-4333-8333-333333333333', 49.55, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'HS SHAMPOO ANTIFALL 180CCX12UN', 61.94, 5.16, 'JAB-HSS-0237', 468, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000238', 'Hs', 'c3333333-3333-4333-8333-333333333333', 49.55, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'HS SHAMPOO LIMP RENOV 180CCX12UN', 61.94, 5.16, 'JAB-HSS-0238', 12, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000239', 'Hs', 'c3333333-3333-4333-8333-333333333333', 82.18, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'HS SHAMPOO LIMP RENOV 375CCX12UN', 102.73, 8.56, 'JAB-HSS-0239', 96, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000240', 'Hs', 'c3333333-3333-4333-8333-333333333333', 82.18, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'HS SHAMPOO C/OLD SPICE 375MLX12UN', 102.73, 8.56, 'JAB-HSS-0240', 180, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000241', 'Hs', 'c3333333-3333-4333-8333-333333333333', 49.55, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'HS SHAMPOO 2en1 SUAVE Y MANEJ 180CCX12UN', 61.94, 5.16, 'JAB-HSS-0241', 264, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000242', 'Hs', 'c3333333-3333-4333-8333-333333333333', 82.18, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'HS SHAMPOO 2en1 SUAVE Y MANEJ 375CCX12UN', 102.73, 8.56, 'JAB-HSS-0242', 348, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000243', 'Hs', 'c3333333-3333-4333-8333-333333333333', 82.18, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'HS SHAMPOO ACEITE DE COCO 375CCX12UN', 102.73, 8.56, 'JAB-HSS-0243', 432, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000244', 'Hs', 'c3333333-3333-4333-8333-333333333333', 82.18, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'HS SHAMPOO ANTI COMEZON 375CCX12UN', 102.73, 8.56, 'JAB-HSS-0244', 516, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000245', 'Hs', 'c3333333-3333-4333-8333-333333333333', 49.55, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'HS SHAMPOO ANTI COMEZON 180CCX12UN', 61.94, 5.16, 'JAB-HSS-0245', 60, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000246', 'Always', 'c3333333-3333-4333-8333-333333333333', 78.51, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'ALWAYS INFINITY NOCT 16 UND 89GX12UN', 98.14, 8.18, 'JAB-ALW-0246', 144, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000247', 'Always', 'c3333333-3333-4333-8333-333333333333', 78.51, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'ALWAYS INFINITY DIA 18 UND 89GX12UN', 98.14, 8.18, 'JAB-ALW-0247', 228, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000248', 'Always', 'c3333333-3333-4333-8333-333333333333', 77.28, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'ALWAYS PROT DIARIOS S/PERF 40UN 90GX24UN', 96.6, 4.03, 'JAB-ALW-0248', 624, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000249', 'Always', 'c3333333-3333-4333-8333-333333333333', 46.55, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'ALWAYS TOALLAS ULT SUAVE NOCHE 43GX24UN', 58.19, 2.42, 'JAB-ALW-0249', 792, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000250', 'Always', 'c3333333-3333-4333-8333-333333333333', 45.66, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'ALWAYS TOALLAS ULTRA SECA DIA 37GX24UN', 57.07, 2.38, 'JAB-ALW-0250', 960, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000251', 'Always', 'c3333333-3333-4333-8333-333333333333', 58.58, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'ALWAYS TOALLAS ULT SUAVE DIA 45GX24UN', 73.22, 3.05, 'JAB-ALW-0251', 48, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000252', 'Always', 'c3333333-3333-4333-8333-333333333333', 50.34, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'ALWAYS TOALLAS ULTRA SECA NOCHE 45GX24UN', 62.92, 2.62, 'JAB-ALW-0252', 216, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000253', 'Always', 'c3333333-3333-4333-8333-333333333333', 44.21, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'ALWAYS TOALLAS ULT SUAVE NOCHE 87GX12UN', 55.26, 4.61, 'JAB-ALW-0253', 192, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000254', 'Always', 'c3333333-3333-4333-8333-333333333333', 25.94, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'ALWAYS TOALL NOCT SEDA F ABUND 101GX12UN', 32.43, 2.7, 'JAB-ALW-0254', 276, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000255', 'Always', 'c3333333-3333-4333-8333-333333333333', 41.65, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'ALWAYS TOALLAS SEDASEC F ABUND 60GX24UN', 52.06, 2.17, 'JAB-ALW-0255', 720, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000256', 'Always', 'c3333333-3333-4333-8333-333333333333', 63.58, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'ALWAYS TOALL NOCT SEDA F ABUND 277GX12UN', 79.48, 6.62, 'JAB-ALW-0256', 444, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000257', 'Gillette', 'c3333333-3333-4333-8333-333333333333', 27.62, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'GILLETTE ANTIT ROLL-ON WAVE 60GX12UN', 34.52, 2.88, 'JAB-GIL-0257', 528, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000258', 'Gillette', 'c3333333-3333-4333-8333-333333333333', 24.83, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'GILLETTE ANTIT ROLL-ON RUSH 60GX12UN', 31.04, 2.59, 'JAB-GIL-0258', 72, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000259', 'Secret', 'c3333333-3333-4333-8333-333333333333', 26.73, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'SECRET ANTIT ROLL-ON PROTECT 60GX12UN', 33.41, 2.78, 'JAB-SEC-0259', 156, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000260', 'MINALBA', 'b2222222-2222-4222-8222-222222222222', 10.96, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'MINALBA SPARKLYN LIMON 500MLX12UN', 13.7, 1.14, 'BEB-MIN-0260', 240, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000261', 'MAZEITE', 'a1111111-1111-4111-8111-111111111111', 46.08, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'MAZEITE 1L X 12UND', 57.6, 4.8, 'ALI-MAZ-0261', 324, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000262', 'RIKESA', 'a1111111-1111-4111-8111-111111111111', 78.67, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 720, 90, 'RIKESA QUESO TOCINETA 330Gx18UN', 98.34, 5.46, 'ALI-RIK-0262', 612, 18);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000263', 'MINALBA', 'b2222222-2222-4222-8222-222222222222', 20.8, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'MINALBA SPARKLING GINGERBEER LAT355ML X24', 26.0, 1.08, 'BEB-MIN-0263', 984, 24);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000264', 'LAS LLAVES', 'a1111111-1111-4111-8111-111111111111', 39.33, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 160, 20, 'LAS LLAVES LINEA ACTIVA 5KG x 4und', 49.16, 12.29, 'ALI-LAS-0264', 12, 4);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000265', 'ORAL B', 'c3333333-3333-4333-8333-333333333333', 5.21, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 160, 20, 'CEPILLO ORAL B INDICAOR X4 RISTRA', 6.51, 1.62, 'JAB-CEP-0265', 40, 4);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000266', 'PEPSI', 'b2222222-2222-4222-8222-222222222222', 4.4, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'PEPSI/SABORES 1,5LX6UN', 5.5, 0.92, 'BEB-PEP-0266', 102, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000267', 'CARORENA', 'b2222222-2222-4222-8222-222222222222', 30.9, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'SANGRIA CAROREÑA TORONJA 1,75Lx6UN', 38.63, 6.43, 'BEB-SAN-0267', 144, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000268', 'HS', 'c3333333-3333-4333-8333-333333333333', 49.55, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'HS ANTIRESEQUEDAD 180MLX12', 61.94, 5.16, 'JAB-HSA-0268', 372, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000269', 'MINALBA', 'b2222222-2222-4222-8222-222222222222', 20.8, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 960, 120, 'MINALBA AGUAKINA LATA 355X24UND', 26.0, 1.08, 'BEB-MIN-0269', 912, 24);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000270', 'PEPSI', 'b2222222-2222-4222-8222-222222222222', 6.4, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'PEPSI 2L X 6UND', 8.0, 1.33, 'BEB-PEP-0270', 6);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000271', 'PEPSI', 'b2222222-2222-4222-8222-222222222222', 6.4, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'PEPSI ZERO 2L X6UND', 8.0, 1.33, 'BEB-PEP-0271', 42, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000272', 'PEPSI', 'b2222222-2222-4222-8222-222222222222', 3.61, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 240, 30, 'PEPSI ZERO 1 L X 6UND', 4.51, 0.75, 'BEB-PEP-0272', 84, 6);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000273', 'LLAVES', 'a1111111-1111-4111-8111-111111111111', 17.72, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 400, 50, 'LLAVES DETG CARICIAS CITRICAS LINEA ACTIVA 900GRX10UN', 22.15, 2.21, 'ALI-LLA-0273', 210, 10);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000274', 'PANTENE', 'c3333333-3333-4333-8333-333333333333', 45.25, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'PTN KERATINA 175GRS', 56.56, 4.88, 'JAB-PTN-0274', 336, 12);
    INSERT INTO "Products" ("Id", "Brand", "CategoryId", "CostPrice", "CreatedAt", "EmptyGroupKey", "IsActive", "LastModifiedAt", "MaxStock", "MinStock", "Name", "PriceBox", "PriceUnit", "SKU", "StockUnits", "UnitsPerBox")
    VALUES ('00000000-0000-4000-8000-000000000275', 'GILLETTE', 'c3333333-3333-4333-8333-333333333333', 33.02, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, TRUE, NULL, 480, 60, 'GILLETTE GEL 42GRS', 41.28, 3.44, 'JAB-GIL-0275', 420, 12);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    CREATE UNIQUE INDEX "IX_Categories_Name" ON "Categories" ("Name") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    CREATE INDEX "IX_Products_CategoryId" ON "Products" ("CategoryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    CREATE INDEX "IX_Products_Name" ON "Products" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    CREATE UNIQUE INDEX "IX_Products_SKU" ON "Products" ("SKU");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261001202014_InitialCatalog') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261001202014_InitialCatalog', '10.0.12');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    ALTER TABLE "Users" ADD "AttendantName" character varying(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    ALTER TABLE "Users" ADD "CanChangeAttendant" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    ALTER TABLE "Products" ADD "IsGiftEligible" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "AuditLogs" (
        "Id" bigint GENERATED ALWAYS AS IDENTITY,
        "Action" character varying(50) NOT NULL,
        "Module" character varying(50) NOT NULL,
        "Description" character varying(1000) NOT NULL,
        "EntityName" character varying(100),
        "EntityId" character varying(50),
        "Amount" numeric(18,2),
        "BeforeJson" jsonb,
        "AfterJson" jsonb,
        "MetadataJson" jsonb,
        "UserId" uuid,
        "Username" character varying(50),
        "Role" character varying(20),
        "IpAddress" inet,
        "CreatedAt" timestamptz NOT NULL DEFAULT (NOW()),
        CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "Clients" (
        "Id" uuid NOT NULL,
        "Code" integer NOT NULL,
        "Rif" character varying(20) NOT NULL,
        "Name" character varying(150) NOT NULL,
        "Address" character varying(250) NOT NULL,
        "Nickname" character varying(100),
        "Type" character varying(30) NOT NULL,
        "MoneyBalance" numeric(18,2) NOT NULL DEFAULT 0.0,
        "EmptyBoxesBalance" integer NOT NULL DEFAULT 0,
        "EmptyUnitsBalance" integer NOT NULL DEFAULT 0,
        "CreatedAt" timestamptz NOT NULL,
        "LastModifiedAt" timestamptz,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_Clients" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "ConsignmentEvents" (
        "Id" uuid NOT NULL,
        "Name" character varying(150) NOT NULL,
        "Responsible" character varying(100) NOT NULL,
        "EventDate" timestamptz NOT NULL,
        "IsClosed" boolean NOT NULL,
        "ClosedAt" timestamptz,
        "CreatedAt" timestamptz NOT NULL,
        "LastModifiedAt" timestamptz,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_ConsignmentEvents" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "DocumentCounters" (
        "Sequence" character varying(30) NOT NULL,
        "Year" integer NOT NULL,
        "Month" integer NOT NULL,
        "CurrentNumber" integer NOT NULL,
        CONSTRAINT "PK_DocumentCounters" PRIMARY KEY ("Sequence", "Year", "Month")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "Employees" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Role" character varying(50) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "LastModifiedAt" timestamptz,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_Employees" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "Trucks" (
        "Id" uuid NOT NULL,
        "Name" character varying(50) NOT NULL,
        "Plate" character varying(20) NOT NULL,
        "Status" character varying(30) NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "LastModifiedAt" timestamptz,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_Trucks" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "ClientEmptyBalances" (
        "ClientId" uuid NOT NULL,
        "GroupKey" character varying(50) NOT NULL,
        "Boxes" integer NOT NULL,
        "Units" integer NOT NULL,
        CONSTRAINT "PK_ClientEmptyBalances" PRIMARY KEY ("ClientId", "GroupKey"),
        CONSTRAINT "FK_ClientEmptyBalances_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "ConsignmentItems" (
        "Id" uuid NOT NULL,
        "EventId" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "ProductName" character varying(150) NOT NULL,
        "PriceBox" numeric(18,2) NOT NULL,
        "PriceUnit" numeric(18,2) NOT NULL,
        "UnitsPerBox" integer NOT NULL,
        "DeliveredUnits" integer NOT NULL,
        "ReturnedUnits" integer NOT NULL,
        CONSTRAINT "PK_ConsignmentItems" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_ConsignmentItems_Delivered_Positive" CHECK ("DeliveredUnits" > 0),
        CONSTRAINT "CK_ConsignmentItems_Returned_Valid" CHECK ("ReturnedUnits" >= 0 AND "ReturnedUnits" <= "DeliveredUnits"),
        CONSTRAINT "FK_ConsignmentItems_ConsignmentEvents_EventId" FOREIGN KEY ("EventId") REFERENCES "ConsignmentEvents" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ConsignmentItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "Purchases" (
        "Id" uuid NOT NULL,
        "Number" character varying(20) NOT NULL,
        "PurchasedAt" timestamptz NOT NULL,
        "EmployeeId" uuid NOT NULL,
        "EmployeeName" character varying(100) NOT NULL,
        "DocumentType" character varying(30) NOT NULL,
        "DocumentNumber" character varying(50),
        "TotalUnits" integer NOT NULL,
        "TotalAmount" numeric(18,2) NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "LastModifiedAt" timestamptz,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_Purchases" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Purchases_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "DamagedProducts" (
        "Id" uuid NOT NULL,
        "Type" character varying(20) NOT NULL,
        "Reason" character varying(20) NOT NULL,
        "ActionTaken" character varying(30) NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "LastModifiedAt" timestamptz,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        "OccurredAt" timestamptz NOT NULL,
        "Origin" character varying(10) NOT NULL,
        "TruckId" uuid,
        "TruckName" character varying(50),
        "ProductId" uuid NOT NULL,
        "ProductName" character varying(150) NOT NULL,
        "Boxes" integer NOT NULL,
        "LooseUnits" integer NOT NULL,
        "TotalUnits" integer NOT NULL,
        "EstimatedValue" numeric(18,2) NOT NULL,
        "Observation" character varying(500),
        CONSTRAINT "PK_DamagedProducts" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_DamagedProducts_TotalUnits_Positive" CHECK ("TotalUnits" > 0),
        CONSTRAINT "FK_DamagedProducts_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_DamagedProducts_Trucks_TruckId" FOREIGN KEY ("TruckId") REFERENCES "Trucks" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "InternalConsumptions" (
        "Id" uuid NOT NULL,
        "Type" character varying(30) NOT NULL,
        "EmployeeId" uuid NOT NULL,
        "EmployeeName" character varying(100) NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "LastModifiedAt" timestamptz,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        "OccurredAt" timestamptz NOT NULL,
        "Origin" character varying(10) NOT NULL,
        "TruckId" uuid,
        "TruckName" character varying(50),
        "ProductId" uuid NOT NULL,
        "ProductName" character varying(150) NOT NULL,
        "Boxes" integer NOT NULL,
        "LooseUnits" integer NOT NULL,
        "TotalUnits" integer NOT NULL,
        "EstimatedValue" numeric(18,2) NOT NULL,
        "Observation" character varying(500),
        CONSTRAINT "PK_InternalConsumptions" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_InternalConsumptions_TotalUnits_Positive" CHECK ("TotalUnits" > 0),
        CONSTRAINT "FK_InternalConsumptions_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_InternalConsumptions_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_InternalConsumptions_Trucks_TruckId" FOREIGN KEY ("TruckId") REFERENCES "Trucks" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "Invoices" (
        "Id" uuid NOT NULL,
        "Number" character varying(60) NOT NULL,
        "Type" character varying(10) NOT NULL,
        "ClientId" uuid NOT NULL,
        "ClientName" character varying(150) NOT NULL,
        "ClientRif" character varying(20) NOT NULL,
        "IssuedAt" timestamptz NOT NULL,
        "DispatchOrigin" character varying(10) NOT NULL,
        "TruckId" uuid,
        "TruckName" character varying(50),
        "TruckPlate" character varying(20),
        "EmployeeId" uuid,
        "AttendantName" character varying(100) NOT NULL,
        "Total" numeric(18,2) NOT NULL,
        "Payment" numeric(18,2) NOT NULL,
        "Pending" numeric(18,2) NOT NULL,
        "PaymentObservation" character varying(1000),
        "GeneratedEmptyBoxes" integer NOT NULL,
        "GeneratedEmptyUnits" integer NOT NULL,
        "ReturnedEmptyBoxes" integer NOT NULL,
        "ReturnedEmptyUnits" integer NOT NULL,
        "IsCancelled" boolean NOT NULL,
        "CancellationReason" character varying(500),
        "CancelledAt" timestamptz,
        "CancelledBy" character varying(50),
        "CreatedAt" timestamptz NOT NULL,
        "LastModifiedAt" timestamptz,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_Invoices" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_Invoices_Gift_NoCharge" CHECK ("Type" <> 'Gift' OR ("Total" = 0 AND "Payment" = 0 AND "Pending" = 0)),
        CONSTRAINT "CK_Invoices_Payment_NotNegative" CHECK ("Payment" >= 0),
        CONSTRAINT "CK_Invoices_Total_NotNegative" CHECK ("Total" >= 0),
        CONSTRAINT "CK_Invoices_Truck_Required" CHECK ("DispatchOrigin" <> 'Truck' OR "TruckId" IS NOT NULL),
        CONSTRAINT "FK_Invoices_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Invoices_Employees_EmployeeId" FOREIGN KEY ("EmployeeId") REFERENCES "Employees" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Invoices_Trucks_TruckId" FOREIGN KEY ("TruckId") REFERENCES "Trucks" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "TruckLoads" (
        "Id" uuid NOT NULL,
        "TruckId" uuid NOT NULL,
        "TruckName" character varying(50) NOT NULL,
        "TruckPlate" character varying(20) NOT NULL,
        "ProductId" uuid NOT NULL,
        "ProductName" character varying(150) NOT NULL,
        "UnitsPerBox" integer NOT NULL,
        "TotalUnits" integer NOT NULL,
        "StockBefore" integer NOT NULL,
        "StockAfter" integer NOT NULL,
        "Source" character varying(30) NOT NULL,
        "SourceTruckId" uuid,
        "Observation" character varying(500),
        "Username" character varying(50),
        "CreatedAt" timestamptz NOT NULL,
        "LastModifiedAt" timestamptz,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_TruckLoads" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TruckLoads_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_TruckLoads_Trucks_TruckId" FOREIGN KEY ("TruckId") REFERENCES "Trucks" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "TruckStock" (
        "TruckId" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "StockUnits" integer NOT NULL,
        CONSTRAINT "PK_TruckStock" PRIMARY KEY ("TruckId", "ProductId"),
        CONSTRAINT "CK_TruckStock_StockUnits_Positive" CHECK ("StockUnits" > 0),
        CONSTRAINT "FK_TruckStock_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_TruckStock_Trucks_TruckId" FOREIGN KEY ("TruckId") REFERENCES "Trucks" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "PurchaseItems" (
        "Id" uuid NOT NULL,
        "PurchaseId" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "ProductName" character varying(150) NOT NULL,
        "PriceBox" numeric(18,2) NOT NULL,
        "PriceUnit" numeric(18,2) NOT NULL,
        "UnitsPerBox" integer NOT NULL,
        "Boxes" integer NOT NULL,
        "LooseUnits" integer NOT NULL,
        "TotalUnits" integer NOT NULL,
        "Subtotal" numeric(18,2) NOT NULL,
        CONSTRAINT "PK_PurchaseItems" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_PurchaseItems_TotalUnits_Positive" CHECK ("TotalUnits" > 0),
        CONSTRAINT "FK_PurchaseItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_PurchaseItems_Purchases_PurchaseId" FOREIGN KEY ("PurchaseId") REFERENCES "Purchases" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "ClientMovements" (
        "Id" uuid NOT NULL,
        "ClientId" uuid NOT NULL,
        "Type" character varying(30) NOT NULL,
        "Title" character varying(100) NOT NULL,
        "Description" character varying(2000) NOT NULL,
        "Amount" numeric(18,2) NOT NULL,
        "InvoiceId" uuid,
        "CreatedAt" timestamptz NOT NULL,
        "LastModifiedAt" timestamptz,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_ClientMovements" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ClientMovements_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_ClientMovements_Invoices_InvoiceId" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "InvoiceEmptyGroups" (
        "InvoiceId" uuid NOT NULL,
        "GroupKey" character varying(50) NOT NULL,
        "GeneratedBoxes" integer NOT NULL,
        "GeneratedUnits" integer NOT NULL,
        "ReturnedBoxes" integer NOT NULL,
        "ReturnedUnits" integer NOT NULL,
        CONSTRAINT "PK_InvoiceEmptyGroups" PRIMARY KEY ("InvoiceId", "GroupKey"),
        CONSTRAINT "FK_InvoiceEmptyGroups_Invoices_InvoiceId" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE TABLE "InvoiceItems" (
        "Id" uuid NOT NULL,
        "InvoiceId" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "ProductName" character varying(150) NOT NULL,
        "SKU" character varying(20) NOT NULL,
        "PriceBox" numeric(18,2) NOT NULL,
        "PriceUnit" numeric(18,2) NOT NULL,
        "UnitsPerBox" integer NOT NULL,
        "EmptyGroupKey" character varying(50),
        "Boxes" integer NOT NULL,
        "LooseUnits" integer NOT NULL,
        "TotalUnits" integer NOT NULL,
        "Subtotal" numeric(18,2) NOT NULL,
        CONSTRAINT "PK_InvoiceItems" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_InvoiceItems_TotalUnits_Positive" CHECK ("TotalUnits" > 0),
        CONSTRAINT "FK_InvoiceItems_Invoices_InvoiceId" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_InvoiceItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    INSERT INTO "Clients" ("Id", "Address", "Code", "CreatedAt", "LastModifiedAt", "Name", "Nickname", "Rif", "Type")
    VALUES ('00000300-0000-4000-8000-000000000001', 'Av. Principal, local 3', 1, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, 'Bodega La Esquina', 'La Esquina', 'J-40000001-0', 'Ocasional');
    INSERT INTO "Clients" ("Id", "Address", "Code", "CreatedAt", "LastModifiedAt", "Name", "Nickname", "Rif", "Type")
    VALUES ('00000300-0000-4000-8000-000000000002', 'Calle 5 con carrera 7', 2, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, 'Abasto El Progreso', 'El Progreso', 'J-40000002-0', 'ESPECIAL');
    INSERT INTO "Clients" ("Id", "Address", "Code", "CreatedAt", "LastModifiedAt", "Name", "Nickname", "Rif", "Type")
    VALUES ('00000300-0000-4000-8000-000000000003', 'Av. Los Andes, local 12', 3, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, 'Licorería Los Andes', 'Los Andes', 'J-40000003-0', 'ESPECIAL');
    INSERT INTO "Clients" ("Id", "Address", "Code", "CreatedAt", "LastModifiedAt", "Name", "Nickname", "Rif", "Type")
    VALUES ('00000300-0000-4000-8000-000000000004', 'Barrio Santa Rosa, calle 2', 4, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, 'Panadería Santa Rosa', 'Santa Rosa', 'V-10000004', 'Ocasional');
    INSERT INTO "Clients" ("Id", "Address", "Code", "CreatedAt", "LastModifiedAt", "Name", "Nickname", "Rif", "Type")
    VALUES ('00000300-0000-4000-8000-000000000005', 'Carrera 10, centro', 5, TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, 'Restaurante El Fogón', 'El Fogón', 'J-40000005-0', 'Ocasional');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    INSERT INTO "Employees" ("Id", "CreatedAt", "IsActive", "LastModifiedAt", "Name", "Role")
    VALUES ('00000100-0000-4000-8000-000000000001', TIMESTAMPTZ '2026-10-01T00:00:00Z', TRUE, NULL, 'Carlos Pérez', 'Vendedor');
    INSERT INTO "Employees" ("Id", "CreatedAt", "IsActive", "LastModifiedAt", "Name", "Role")
    VALUES ('00000100-0000-4000-8000-000000000002', TIMESTAMPTZ '2026-10-01T00:00:00Z', TRUE, NULL, 'María González', 'Vendedora');
    INSERT INTO "Employees" ("Id", "CreatedAt", "IsActive", "LastModifiedAt", "Name", "Role")
    VALUES ('00000100-0000-4000-8000-000000000003', TIMESTAMPTZ '2026-10-01T00:00:00Z', TRUE, NULL, 'Luis Hernández', 'Vendedor');
    INSERT INTO "Employees" ("Id", "CreatedAt", "IsActive", "LastModifiedAt", "Name", "Role")
    VALUES ('00000100-0000-4000-8000-000000000004', TIMESTAMPTZ '2026-10-01T00:00:00Z', TRUE, NULL, 'José Rodríguez', 'Despachador');
    INSERT INTO "Employees" ("Id", "CreatedAt", "IsActive", "LastModifiedAt", "Name", "Role")
    VALUES ('00000100-0000-4000-8000-000000000005', TIMESTAMPTZ '2026-10-01T00:00:00Z', TRUE, NULL, 'Pedro Ramírez', 'Chofer');
    INSERT INTO "Employees" ("Id", "CreatedAt", "IsActive", "LastModifiedAt", "Name", "Role")
    VALUES ('00000100-0000-4000-8000-000000000006', TIMESTAMPTZ '2026-10-01T00:00:00Z', TRUE, NULL, 'Ana Martínez', 'Administración');
    INSERT INTO "Employees" ("Id", "CreatedAt", "IsActive", "LastModifiedAt", "Name", "Role")
    VALUES ('00000100-0000-4000-8000-000000000007', TIMESTAMPTZ '2026-10-01T00:00:00Z', TRUE, NULL, 'Laura Torres', 'Vendedora');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    UPDATE "Products" SET "IsGiftEligible" = TRUE
    WHERE "Id" = '00000000-0000-4000-8000-000000000001';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    UPDATE "Products" SET "IsGiftEligible" = TRUE
    WHERE "Id" = '00000000-0000-4000-8000-000000000004';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    UPDATE "Products" SET "IsGiftEligible" = TRUE
    WHERE "Id" = '00000000-0000-4000-8000-000000000006';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    UPDATE "Products" SET "IsGiftEligible" = TRUE
    WHERE "Id" = '00000000-0000-4000-8000-000000000012';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    UPDATE "Products" SET "IsGiftEligible" = TRUE
    WHERE "Id" = '00000000-0000-4000-8000-000000000021';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    UPDATE "Products" SET "IsGiftEligible" = TRUE
    WHERE "Id" = '00000000-0000-4000-8000-000000000022';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    INSERT INTO "Trucks" ("Id", "CreatedAt", "LastModifiedAt", "Name", "Plate", "Status")
    VALUES ('00000200-0000-4000-8000-000000000001', TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, 'Cargo 815', 'AA100AA', 'En galpón');
    INSERT INTO "Trucks" ("Id", "CreatedAt", "LastModifiedAt", "Name", "Plate", "Status")
    VALUES ('00000200-0000-4000-8000-000000000002', TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, 'FVR', 'AA200AA', 'En galpón');
    INSERT INTO "Trucks" ("Id", "CreatedAt", "LastModifiedAt", "Name", "Plate", "Status")
    VALUES ('00000200-0000-4000-8000-000000000003', TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, 'Mack', 'AA300AA', 'En galpón');
    INSERT INTO "Trucks" ("Id", "CreatedAt", "LastModifiedAt", "Name", "Plate", "Status")
    VALUES ('00000200-0000-4000-8000-000000000004', TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, 'Kodiak', 'AA400AA', 'En galpón');
    INSERT INTO "Trucks" ("Id", "CreatedAt", "LastModifiedAt", "Name", "Plate", "Status")
    VALUES ('00000200-0000-4000-8000-000000000005', TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, 'F-350', 'AA500AA', 'En galpón');
    INSERT INTO "Trucks" ("Id", "CreatedAt", "LastModifiedAt", "Name", "Plate", "Status")
    VALUES ('00000200-0000-4000-8000-000000000006', TIMESTAMPTZ '2026-10-01T00:00:00Z', NULL, 'Montana', 'AA600AA', 'En galpón');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_AuditLogs_CreatedAt" ON "AuditLogs" ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_AuditLogs_Module_CreatedAt" ON "AuditLogs" ("Module", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_ClientMovements_ClientId_CreatedAt" ON "ClientMovements" ("ClientId", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_ClientMovements_InvoiceId" ON "ClientMovements" ("InvoiceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_ClientMovements_Type_CreatedAt" ON "ClientMovements" ("Type", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE UNIQUE INDEX "IX_Clients_Code" ON "Clients" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_Clients_Name" ON "Clients" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE UNIQUE INDEX "IX_Clients_Rif" ON "Clients" ("Rif") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_ConsignmentEvents_IsClosed" ON "ConsignmentEvents" ("IsClosed");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_ConsignmentItems_EventId" ON "ConsignmentItems" ("EventId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_ConsignmentItems_ProductId" ON "ConsignmentItems" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_DamagedProducts_OccurredAt" ON "DamagedProducts" ("OccurredAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_DamagedProducts_ProductId" ON "DamagedProducts" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_DamagedProducts_TruckId" ON "DamagedProducts" ("TruckId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_Employees_Name" ON "Employees" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_InternalConsumptions_EmployeeId" ON "InternalConsumptions" ("EmployeeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_InternalConsumptions_OccurredAt" ON "InternalConsumptions" ("OccurredAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_InternalConsumptions_ProductId" ON "InternalConsumptions" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_InternalConsumptions_TruckId" ON "InternalConsumptions" ("TruckId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_InvoiceItems_InvoiceId" ON "InvoiceItems" ("InvoiceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_InvoiceItems_ProductId" ON "InvoiceItems" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_Invoices_ClientId_IssuedAt" ON "Invoices" ("ClientId", "IssuedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_Invoices_EmployeeId" ON "Invoices" ("EmployeeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_Invoices_IssuedAt" ON "Invoices" ("IssuedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE UNIQUE INDEX "IX_Invoices_Number" ON "Invoices" ("Number");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_Invoices_TruckId" ON "Invoices" ("TruckId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_PurchaseItems_ProductId" ON "PurchaseItems" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_PurchaseItems_PurchaseId" ON "PurchaseItems" ("PurchaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_Purchases_EmployeeId" ON "Purchases" ("EmployeeId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE UNIQUE INDEX "IX_Purchases_Number" ON "Purchases" ("Number");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_Purchases_PurchasedAt" ON "Purchases" ("PurchasedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_TruckLoads_ProductId" ON "TruckLoads" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_TruckLoads_TruckId_CreatedAt" ON "TruckLoads" ("TruckId", "CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE UNIQUE INDEX "IX_Trucks_Plate" ON "Trucks" ("Plate") WHERE "IsDeleted" = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    CREATE INDEX "IX_TruckStock_ProductId" ON "TruckStock" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261002060415_BusinessOperations') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261002060415_BusinessOperations', '10.0.12');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015144424_TruckCapacity') THEN
    ALTER TABLE "Trucks" ADD "CapacityBoxes" integer NOT NULL DEFAULT 500;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015144424_TruckCapacity') THEN
    UPDATE "Trucks" SET "CapacityBoxes" = 600
    WHERE "Id" = '00000200-0000-4000-8000-000000000001';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015144424_TruckCapacity') THEN
    UPDATE "Trucks" SET "CapacityBoxes" = 500
    WHERE "Id" = '00000200-0000-4000-8000-000000000002';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015144424_TruckCapacity') THEN
    UPDATE "Trucks" SET "CapacityBoxes" = 900
    WHERE "Id" = '00000200-0000-4000-8000-000000000003';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015144424_TruckCapacity') THEN
    UPDATE "Trucks" SET "CapacityBoxes" = 700
    WHERE "Id" = '00000200-0000-4000-8000-000000000004';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015144424_TruckCapacity') THEN
    UPDATE "Trucks" SET "CapacityBoxes" = 150
    WHERE "Id" = '00000200-0000-4000-8000-000000000005';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015144424_TruckCapacity') THEN
    UPDATE "Trucks" SET "CapacityBoxes" = 60
    WHERE "Id" = '00000200-0000-4000-8000-000000000006';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015144424_TruckCapacity') THEN
    ALTER TABLE "Trucks" ADD CONSTRAINT "CK_Trucks_CapacityBoxes_Range" CHECK ("CapacityBoxes" BETWEEN 1 AND 5000);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015144424_TruckCapacity') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261015144424_TruckCapacity', '10.0.12');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    ALTER TABLE "Trucks" ADD "DispatchedAt" timestamptz;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    ALTER TABLE "Invoices" ADD "IdempotencyKey" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    ALTER TABLE "Clients" ADD "CreditLimit" numeric(18,2);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    CREATE TABLE "RouteSettlements" (
        "Id" uuid NOT NULL,
        "TruckId" uuid NOT NULL,
        "TruckName" character varying(50) NOT NULL,
        "TruckPlate" character varying(20) NOT NULL,
        "DispatchedAt" timestamptz NOT NULL,
        "SettledAt" timestamptz NOT NULL,
        "SettledBy" character varying(50),
        "Observation" character varying(500),
        "InvoicesCount" integer NOT NULL,
        "SalesTotal" numeric(18,2) NOT NULL,
        "ExpectedCash" numeric(18,2) NOT NULL,
        "CountedCash" numeric(18,2) NOT NULL,
        "CashDifference" numeric(18,2) NOT NULL,
        "MissingGoodsValue" numeric(18,2) NOT NULL,
        "CreatedAt" timestamptz NOT NULL,
        "LastModifiedAt" timestamptz,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_RouteSettlements" PRIMARY KEY ("Id"),
        CONSTRAINT "CK_RouteSettlements_CountedCash_NotNegative" CHECK ("CountedCash" >= 0),
        CONSTRAINT "FK_RouteSettlements_Trucks_TruckId" FOREIGN KEY ("TruckId") REFERENCES "Trucks" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    CREATE TABLE "RouteSettlementLines" (
        "Id" uuid NOT NULL,
        "SettlementId" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "ProductName" character varying(150) NOT NULL,
        "UnitsPerBox" integer NOT NULL,
        "PriceUnit" numeric(18,2) NOT NULL,
        "ExpectedUnits" integer NOT NULL,
        "CountedUnits" integer NOT NULL,
        "DifferenceUnits" integer NOT NULL,
        "DifferenceValue" numeric(18,2) NOT NULL,
        CONSTRAINT "PK_RouteSettlementLines" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RouteSettlementLines_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_RouteSettlementLines_RouteSettlements_SettlementId" FOREIGN KEY ("SettlementId") REFERENCES "RouteSettlements" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    UPDATE "Clients" SET "CreditLimit" = NULL
    WHERE "Id" = '00000300-0000-4000-8000-000000000001';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    UPDATE "Clients" SET "CreditLimit" = NULL
    WHERE "Id" = '00000300-0000-4000-8000-000000000002';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    UPDATE "Clients" SET "CreditLimit" = NULL
    WHERE "Id" = '00000300-0000-4000-8000-000000000003';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    UPDATE "Clients" SET "CreditLimit" = NULL
    WHERE "Id" = '00000300-0000-4000-8000-000000000004';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    UPDATE "Clients" SET "CreditLimit" = NULL
    WHERE "Id" = '00000300-0000-4000-8000-000000000005';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    UPDATE "Trucks" SET "DispatchedAt" = NULL
    WHERE "Id" = '00000200-0000-4000-8000-000000000001';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    UPDATE "Trucks" SET "DispatchedAt" = NULL
    WHERE "Id" = '00000200-0000-4000-8000-000000000002';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    UPDATE "Trucks" SET "DispatchedAt" = NULL
    WHERE "Id" = '00000200-0000-4000-8000-000000000003';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    UPDATE "Trucks" SET "DispatchedAt" = NULL
    WHERE "Id" = '00000200-0000-4000-8000-000000000004';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    UPDATE "Trucks" SET "DispatchedAt" = NULL
    WHERE "Id" = '00000200-0000-4000-8000-000000000005';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    UPDATE "Trucks" SET "DispatchedAt" = NULL
    WHERE "Id" = '00000200-0000-4000-8000-000000000006';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    CREATE UNIQUE INDEX "IX_Invoices_IdempotencyKey" ON "Invoices" ("IdempotencyKey") WHERE "IdempotencyKey" IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    ALTER TABLE "Clients" ADD CONSTRAINT "CK_Clients_CreditLimit_NotNegative" CHECK ("CreditLimit" IS NULL OR "CreditLimit" >= 0);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    CREATE INDEX "IX_RouteSettlementLines_ProductId" ON "RouteSettlementLines" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    CREATE INDEX "IX_RouteSettlementLines_SettlementId" ON "RouteSettlementLines" ("SettlementId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    CREATE INDEX "IX_RouteSettlements_TruckId_SettledAt" ON "RouteSettlements" ("TruckId", "SettledAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261015205223_ProfessorObservations') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261015205223_ProfessorObservations', '10.0.12');
    END IF;
END $EF$;
COMMIT;

