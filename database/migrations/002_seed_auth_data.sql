-- Seed data for testing multi-company authentication
-- Run this after 001_init_schema.sql

-- Insert test roles
INSERT INTO roles (id, name, description, is_system_role) VALUES
('11111111-1111-1111-1111-111111111111', 'admin', 'Administrator with full access', true),
('22222222-2222-2222-2222-222222222222', 'user', 'Standard user', true),
('33333333-3333-3333-3333-333333333333', 'accountant', 'Accountant role', true)
ON CONFLICT (name) DO NOTHING;

-- Insert test company (if not exists)
INSERT INTO companies (id, ruc, business_name, trade_name, email, status) VALUES
('123e4567-e89b-12d3-a456-426614174000', '1234567890001', 'Empresa Demo S.A.', 'Demo Company', 'empresa@demo.com', 'active')
ON CONFLICT (id) DO NOTHING;

-- Insert additional test company
INSERT INTO companies (id, ruc, business_name, trade_name, email, status) VALUES
('223e4567-e89b-12d3-a456-426614174000', '0987654321001', 'Segunda Empresa S.A.', 'Second Company', 'empresa2@demo.com', 'active')
ON CONFLICT (ruc) DO NOTHING;

-- Insert test users with hashed passwords
-- Password for all users: "password123"
INSERT INTO users (id, email, password_hash, full_name, phone, status) VALUES
('a1111111-1111-1111-1111-111111111111', 'admin@demo.com', '$2a$10$G.C9xGqLCT7lzmV9Yvfic.6jpBDgC.scOqs3dQpvw2MSKBXUbfq0y', 'Administrador Demo', '0999999999', 'active'),
('b2222222-2222-2222-2222-222222222222', 'user@demo.com', '$2a$10$G.C9xGqLCT7lzmV9Yvfic.6jpBDgC.scOqs3dQpvw2MSKBXUbfq0y', 'Usuario Demo', '0988888888', 'active'),
('c3333333-3333-3333-3333-333333333333', 'multi@demo.com', '$2a$10$G.C9xGqLCT7lzmV9Yvfic.6jpBDgC.scOqs3dQpvw2MSKBXUbfq0y', 'Usuario Multi-Company', '0977777777', 'active')
ON CONFLICT (email) DO NOTHING;

-- Assign users to companies with roles
-- Admin user - Company 1
INSERT INTO user_company_roles (user_id, company_id, role_id) VALUES
('a1111111-1111-1111-1111-111111111111', '123e4567-e89b-12d3-a456-426614174000', '11111111-1111-1111-1111-111111111111')
ON CONFLICT (user_id, company_id, role_id) DO NOTHING;

-- Regular user - Company 1
INSERT INTO user_company_roles (user_id, company_id, role_id) VALUES
('b2222222-2222-2222-2222-222222222222', '123e4567-e89b-12d3-a456-426614174000', '22222222-2222-2222-2222-222222222222')
ON CONFLICT (user_id, company_id, role_id) DO NOTHING;

-- Multi-company user - Both companies
INSERT INTO user_company_roles (user_id, company_id, role_id) VALUES
('c3333333-3333-3333-3333-333333333333', '123e4567-e89b-12d3-a456-426614174000', '11111111-1111-1111-1111-111111111111'),
('c3333333-3333-3333-3333-333333333333', '223e4567-e89b-12d3-a456-426614174000', '33333333-3333-3333-3333-333333333333')
ON CONFLICT (user_id, company_id, role_id) DO NOTHING;

-- Display created users
SELECT 
  u.email,
  u.full_name,
  'password123' as password,
  c.business_name as company,
  r.name as role
FROM users u
JOIN user_company_roles ucr ON ucr.user_id = u.id
JOIN companies c ON c.id = ucr.company_id
JOIN roles r ON r.id = ucr.role_id
ORDER BY u.email, c.business_name;
