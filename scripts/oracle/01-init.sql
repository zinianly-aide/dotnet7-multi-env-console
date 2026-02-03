-- Initialize Oracle database for development

-- Create Products table
CREATE TABLE Products (
    Id NUMBER(10) GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Name VARCHAR2(200) NOT NULL,
    Description CLOB,
    Price NUMBER(18,2) DEFAULT 0,
    Stock NUMBER(10) DEFAULT 0,
    CreatedAt TIMESTAMP DEFAULT SYSTIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT SYSTIMESTAMP
);

-- Create index on Name
CREATE INDEX IDX_PRODUCTS_NAME ON Products(Name);

-- Insert sample data
INSERT INTO Products (Name, Description, Price, Stock)
VALUES (
    'Oracle Database',
    'Enterprise-grade database management system',
    9999.99,
    10
);

INSERT INTO Products (Name, Description, Price, Stock)
VALUES (
    'Enterprise Server',
    'High-performance server for enterprise applications',
    5999.99,
    25
);

COMMIT;

-- Create audit log table
CREATE TABLE AuditLogs (
    Id NUMBER(20) GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    Timestamp TIMESTAMP DEFAULT SYSTIMESTAMP,
    Level VARCHAR2(20),
    Message CLOB,
    UserId VARCHAR2(100),
    Action VARCHAR2(50),
    Entity VARCHAR2(100),
    EntityId NUMBER(10),
    Details CLOB
);

-- Create indexes
CREATE INDEX IDX_AUDITLOGS_TIMESTAMP ON AuditLogs(Timestamp);
CREATE INDEX IDX_AUDITLOGS_LEVEL ON AuditLogs(Level);
CREATE INDEX IDX_AUDITLOGS_ACTION ON AuditLogs(Action);

COMMIT;
