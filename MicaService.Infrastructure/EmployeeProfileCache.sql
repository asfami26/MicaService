CREATE TABLE EMP.EmployeeProfileCache (
    CacheId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    EmployeeId NVARCHAR(50) NOT NULL,
    DepartmentId NVARCHAR(50) NULL,
    DivisionId NVARCHAR(50) NULL,
    FullName NVARCHAR(200) NULL,
    Age INT NOT NULL,
    Status NVARCHAR(50) NULL,
    PositionName NVARCHAR(200) NULL,
    JgcGrade NVARCHAR(50) NULL,
    JoinDate DATETIME NULL,
    NoKtp NVARCHAR(100) NULL,
    CurrentAddress NVARCHAR(400) NULL,
    Email NVARCHAR(200) NULL,
    PersonalEmail NVARCHAR(200) NULL,
    Tax NVARCHAR(50) NULL,
    Education1 NVARCHAR(200) NULL,
    Edu1GradYear DATETIME NULL,
    Edu1Majoring NVARCHAR(200) NULL,
    Education2 NVARCHAR(200) NULL,
    Edu2GradYear DATETIME NULL,
    Edu2Majoring NVARCHAR(200) NULL,
    Yos INT NOT NULL,
    AllExp INT NOT NULL
);

CREATE INDEX IX_EmployeeProfileCache_DepartmentId
    ON EMP.EmployeeProfileCache (DepartmentId);

CREATE INDEX IX_EmployeeProfileCache_EmployeeId
    ON EMP.EmployeeProfileCache (EmployeeId);
