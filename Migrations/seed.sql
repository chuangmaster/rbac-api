-- ============================================
-- RBAC 中央授權系統 - Seed Data
-- 密碼: P@ssw0rd (BCrypt hash)
-- ============================================

-- 1. 權限 (嚴格遵循 [module]:[resource]:[action] 規範，不含角色名稱)
INSERT INTO permissions (code, description) VALUES
    ('transport:portal:admin',   '交通模組 - 管理員等級門戶存取'),
    ('transport:portal:access',  '交通模組 - 一般門戶存取'),
    ('transport:report:export',  '交通模組 - 報表匯出'),
    ('transport:order:delete',   '交通模組 - 訂單刪除'),
    ('bento:portal:access',      '便當模組 - 門戶存取'),
    ('bento:order:create',       '便當模組 - 建立訂單'),
    ('announcement:admin',       '公告模組 - 管理（新增/編輯/刪除）')
ON CONFLICT (code) DO NOTHING;

-- 2. 角色
INSERT INTO roles (name, description) VALUES
    ('system_admin',    '系統管理員 - 擁有全部權限'),
    ('transport_admin', '交通管理員 - 交通模組全部權限'),
    ('bento_user',      '便當使用者 - 便當模組基本權限')
ON CONFLICT (name) DO NOTHING;

-- 3. 角色-權限對應
-- system_admin → 全部權限
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r, permissions p
WHERE r.name = 'system_admin'
ON CONFLICT DO NOTHING;

-- transport_admin → transport:* 權限
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r, permissions p
WHERE r.name = 'transport_admin' AND p.code LIKE 'transport:%'
ON CONFLICT DO NOTHING;

-- bento_user → bento:* 權限
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r, permissions p
WHERE r.name = 'bento_user' AND p.code LIKE 'bento:%'
ON CONFLICT DO NOTHING;

-- 4. 使用者
-- 密碼: P@ssw0rd → BCrypt hash (cost=11)
INSERT INTO users (employee_id, name, password_hash) VALUES
    ('EMP-0001', 'Admin',  '$2a$11$6uabNMGS7zustuBOOwpskOYVMGiOvTdhsFKN5DH5M90.AMP3EXKBm'),
    ('EMP-0002', 'Zander', '$2a$11$6uabNMGS7zustuBOOwpskOYVMGiOvTdhsFKN5DH5M90.AMP3EXKBm')
ON CONFLICT (employee_id) DO NOTHING;

-- 5. 使用者-角色對應
-- Admin (EMP-0001) → system_admin
INSERT INTO user_roles (user_id, role_id)
SELECT u.id, r.id
FROM users u, roles r
WHERE u.employee_id = 'EMP-0001' AND r.name = 'system_admin'
ON CONFLICT DO NOTHING;

-- Zander (EMP-0002) → bento_user
INSERT INTO user_roles (user_id, role_id)
SELECT u.id, r.id
FROM users u, roles r
WHERE u.employee_id = 'EMP-0002' AND r.name = 'bento_user'
ON CONFLICT DO NOTHING;
