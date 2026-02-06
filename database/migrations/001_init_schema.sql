-- Initial database schema for Billing & Inventory System
-- Based on docs/database-schema.md

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =============================================
-- MULTI-TENANCY & AUTHENTICATION
-- =============================================

CREATE TABLE companies (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    ruc VARCHAR(13) NOT NULL UNIQUE,
    business_name VARCHAR(255) NOT NULL,
    trade_name VARCHAR(255),
    address TEXT,
    phone VARCHAR(20),
    email VARCHAR(255),
    accounting_required BOOLEAN DEFAULT false,
    special_taxpayer_number VARCHAR(20),
    sri_environment INTEGER DEFAULT 1,
    digital_certificate_path TEXT,
    certificate_password TEXT,
    status VARCHAR(20) DEFAULT 'active',
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_companies_ruc ON companies(ruc);
CREATE INDEX idx_companies_status ON companies(status);

-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(255) NOT NULL,
    phone VARCHAR(20),
    status VARCHAR(20) DEFAULT 'active',
    last_login_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_status ON users(status);

-- Roles table
CREATE TABLE roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(50) NOT NULL UNIQUE,
    description TEXT,
    permissions JSONB DEFAULT '[]'::jsonb,
    is_system_role BOOLEAN DEFAULT false,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_roles_name ON roles(name);

-- User-Company-Roles junction table
CREATE TABLE user_company_roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    assigned_at TIMESTAMP DEFAULT NOW(),
    assigned_by UUID REFERENCES users(id),
    UNIQUE(user_id, company_id, role_id)
);

CREATE INDEX idx_ucr_user_company ON user_company_roles(user_id, company_id);
CREATE INDEX idx_ucr_company ON user_company_roles(company_id);

-- =============================================
-- TAX CONFIGURATION (Ecuador SRI)
-- =============================================

CREATE TABLE tax_types (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    code VARCHAR(10) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    rate DECIMAL(5,2) NOT NULL,
    sri_code VARCHAR(10) NOT NULL,
    active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_tax_types_code ON tax_types(code);
CREATE INDEX idx_tax_types_active ON tax_types(active);

-- Insert default tax types
INSERT INTO tax_types (code, name, rate, sri_code) VALUES
('IVA_12', 'IVA 12%', 12.00, '2'),
('IVA_0', 'IVA 0%', 0.00, '0'),
('NO_IVA', 'No IVA', 0.00, '6'),
('IVA_EXEMPT', 'IVA Exento', 0.00, '7');

CREATE TABLE retention_types (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    type VARCHAR(10) NOT NULL,
    code VARCHAR(10) NOT NULL,
    description VARCHAR(255) NOT NULL,
    rate DECIMAL(5,2) NOT NULL,
    sri_code VARCHAR(10) NOT NULL,
    active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_retention_types_type ON retention_types(type);
CREATE INDEX idx_retention_types_active ON retention_types(active);

CREATE TABLE payment_methods (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL,
    sri_code VARCHAR(2) NOT NULL UNIQUE,
    active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_payment_methods_sri_code ON payment_methods(sri_code);

-- Insert default payment methods
INSERT INTO payment_methods (name, sri_code) VALUES
('Efectivo', '01'),
('Tarjeta de crédito', '19'),
('Otros con utilización del sistema financiero', '20');

-- =============================================
-- PRODUCTS & INVENTORY
-- =============================================

CREATE TABLE products (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    code VARCHAR(50) NOT NULL,
    barcode VARCHAR(100),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    category VARCHAR(100),
    unit VARCHAR(10) NOT NULL,
    price DECIMAL(10,2) NOT NULL DEFAULT 0,
    cost DECIMAL(10,2) NOT NULL DEFAULT 0,
    tax_type_id UUID NOT NULL REFERENCES tax_types(id),
    has_inventory BOOLEAN DEFAULT true,
    min_stock INTEGER,
    max_stock INTEGER,
    reorder_point INTEGER,
    image TEXT,
    status VARCHAR(20) DEFAULT 'active',
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(company_id, code)
);

CREATE INDEX idx_products_company ON products(company_id);
CREATE INDEX idx_products_code ON products(code);
CREATE INDEX idx_products_barcode ON products(barcode);
CREATE INDEX idx_products_name ON products(name);
CREATE INDEX idx_products_category ON products(category);
CREATE INDEX idx_products_status ON products(status);

-- =============================================
-- CUSTOMERS
-- =============================================

CREATE TABLE customers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    company_id UUID NOT NULL REFERENCES companies(id) ON DELETE CASCADE,
    identification_type VARCHAR(10) NOT NULL,
    identification VARCHAR(13) NOT NULL,
    business_name VARCHAR(255) NOT NULL,
    trade_name VARCHAR(255),
    email VARCHAR(255),
    phone VARCHAR(20),
    mobile VARCHAR(20),
    address TEXT,
    city VARCHAR(100),
    province VARCHAR(100),
    country VARCHAR(2) DEFAULT 'EC',
    credit_limit DECIMAL(10,2) DEFAULT 0,
    credit_days INTEGER DEFAULT 0,
    notes TEXT,
    status VARCHAR(20) DEFAULT 'active',
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    UNIQUE(company_id, identification)
);

CREATE INDEX idx_customers_company ON customers(company_id);
CREATE INDEX idx_customers_identification ON customers(identification);
CREATE INDEX idx_customers_name ON customers(business_name);
CREATE INDEX idx_customers_status ON customers(status);

-- NOTE: Additional tables for invoices, inventory movements, etc.
-- will be added in subsequent migrations as we build the system.
