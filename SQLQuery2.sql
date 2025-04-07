
CREATE TABLE Favorites (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    CourseId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES [Users](Id),
    FOREIGN KEY (CourseId) REFERENCES Lessons(Id) -- Assuming Course table exists
);

ALTER TABLE Lessons
ADD LessonState NVARCHAR(50);



CREATE TABLE Packages (
    PackageId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL
);

CREATE TABLE PackageLesson (
    PackageLessonId INT IDENTITY(1,1) PRIMARY KEY,
    PackageId INT NOT NULL,
    LessonId INT NOT NULL,
    FOREIGN KEY (PackageId) REFERENCES Packages(PackageId),
    FOREIGN KEY (LessonId) REFERENCES Lessons(Id)
);

CREATE TABLE FreeFiles
(
    FileId INT IDENTITY(1,1) PRIMARY KEY,  -- مفتاح أساسي مع زيادة تلقائية
    Title NVARCHAR(255) NOT NULL,          -- العنوان، مطلوب بحد أقصى 255 حرف
    Description NVARCHAR(MAX) NULL,        -- الوصف، اختياري
    Url NVARCHAR(2083) NULL                 -- الرابط، اختياري بحد أقصى 2083 حرف
);

-- إنشاء جدول FreeVideos
CREATE TABLE FreeVideos
(
    VideoId INT IDENTITY(1,1) PRIMARY KEY, -- مفتاح أساسي مع زيادة تلقائية
    Title NVARCHAR(255) NOT NULL,          -- العنوان، مطلوب بحد أقصى 255 حرف
    Description NVARCHAR(MAX) NULL,        -- الوصف، اختياري
    Url NVARCHAR(2083) NULL                 -- الرابط، اختياري بحد أقصى 2083 حرف
);

-- إنشاء جدول FreeProjects
CREATE TABLE FreeProjects
(
    ProjectId INT IDENTITY(1,1) PRIMARY KEY, -- مفتاح أساسي مع زيادة تلقائية
    Title NVARCHAR(255) NOT NULL,            -- العنوان، مطلوب بحد أقصى 255 حرف
    Description NVARCHAR(MAX) NULL,          -- الوصف، اختياري
    Url NVARCHAR(2083) NULL                   -- الرابط، اختياري بحد أقصى 2083 حرف
);

CREATE TABLE FreeBooks
(
    BookId INT IDENTITY(1,1) PRIMARY KEY, -- مفتاح أساسي مع زيادة تلقائية
    Title NVARCHAR(255) NOT NULL,            -- العنوان، مطلوب بحد أقصى 255 حرف
    Description NVARCHAR(MAX) NULL,          -- الوصف، اختياري
    Url NVARCHAR(2083) NULL                   -- الرابط، اختياري بحد أقصى 2083 حرف
);

CREATE TABLE LessonFiles
(
    FileId INT IDENTITY(1,1) PRIMARY KEY,  -- مفتاح أساسي مع زيادة تلقائية
    LessonId INT NOT NULL,                 -- معرف الدرس المرتبط
    Title NVARCHAR(255) NOT NULL,          -- عنوان الملف
    Description NVARCHAR(MAX) NULL,        -- الوصف، اختياري
    Url NVARCHAR(2083) NULL,               -- رابط الملف

    -- تعريف علاقة الربط بين الملفات والدروس
    CONSTRAINT FK_LessonFiles_Lessons FOREIGN KEY (LessonId)
    REFERENCES Lessons (Id) ON DELETE CASCADE
);

CREATE TABLE LectureFiles
(
    FileId INT IDENTITY(1,1) PRIMARY KEY,  -- مفتاح أساسي مع زيادة تلقائية
    LectureId INT NOT NULL,                -- معرف المحاضرة المرتبط
    Title NVARCHAR(255) NOT NULL,          -- عنوان الملف
    Description NVARCHAR(MAX) NULL,        -- الوصف، اختياري
    Url NVARCHAR(2083) NULL,               -- رابط الملف

    -- تعريف علاقة الربط بين الملفات والمحاضرات
    CONSTRAINT FK_LectureFiles_Lectures FOREIGN KEY (LectureId)
    REFERENCES Lectures (Id) ON DELETE CASCADE
);

ALTER TABLE Lessons
ADD TeacherId INT,  -- تضيف عمود TeacherId من نوع INT
CONSTRAINT FK_Lessons_Users -- تحدد اسم المفتاح الأجنبي
FOREIGN KEY (TeacherId) REFERENCES Users(Id);

CREATE TABLE LatestOffers (
    OfferId INT IDENTITY(1,1) PRIMARY KEY,                          -- المفتاح الأساسي للجدول
    OfferDescription NVARCHAR(255) NOT NULL,          -- وصف العرض، لا يقبل القيم الفارغة
    OfferUrl NVARCHAR(2083),                          -- رابط العرض، يقبل القيم الفارغة
    Title NVARCHAR(255)                               -- عمود العنوان الجديد، يمكن تخصيصه حسب الحاجة
);


ALTER TABLE LatestOffers ADD FilePath VARCHAR(255);
ALTER TABLE Packages ADD FilePath VARCHAR(255);

SELECT * FROM BlockedLectures;

ALTER TABLE Users ADD FailedLoginAttempts  int ;

ALTER TABLE Users ADD LockoutEndTime   DateTime;
select * from Users where Id = 4766
select * from blockedLectures where UserId = 4766;  
drop table RefreshTokens;
CREATE TABLE RefreshTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,  -- الرقم التسلسلي للمفتاح الأساسي
    Token NVARCHAR(255) NOT NULL,       -- التوكن نفسه
    UserId int NOT NULL,      -- معرف المستخدم المرتبط بالتوكن
    Expiration DATETIME NOT NULL,       -- تاريخ انتهاء صلاحية التوكن
    IsRevoked BIT NOT NULL DEFAULT 0    -- حالة التوكن (هل تم إبطاله أم لا)
);
