-- Initialize MySQL database for development

CREATE DATABASE IF NOT EXISTS devdb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE devdb;

-- Create Products table
CREATE TABLE IF NOT EXISTS Products (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Description TEXT,
    Price DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    Stock INT NOT NULL DEFAULT 0,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_name (Name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert sample data
INSERT INTO Products (Name, Description, Price, Stock) VALUES
('Laptop', 'High-performance laptop with 16GB RAM', 1299.99, 50),
('Mouse', 'Wireless optical mouse', 29.99, 200),
('Keyboard', 'Mechanical keyboard with RGB lighting', 89.99, 150),
('Monitor', '27-inch 4K IPS display', 399.99, 75),
('Headphones', 'Noise-cancelling wireless headphones', 249.99, 100)
ON DUPLICATE KEY UPDATE UpdatedAt = CURRENT_TIMESTAMP;

-- Create audit log table
CREATE TABLE IF NOT EXISTS AuditLogs (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    Timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Level VARCHAR(20),
    Message TEXT,
    UserId VARCHAR(100),
    Action VARCHAR(50),
    Entity VARCHAR(100),
    EntityId INT,
    Details JSON,
    INDEX idx_timestamp (Timestamp),
    INDEX idx_level (Level),
    INDEX idx_action (Action)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
