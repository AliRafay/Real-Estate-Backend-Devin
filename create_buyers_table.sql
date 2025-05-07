CREATE TABLE IF NOT EXISTS public."Buyers" (
    "Id" SERIAL PRIMARY KEY,
    "FirstName" TEXT NOT NULL,
    "LastName" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "PhoneNumber" TEXT,
    "Address" TEXT,
    "City" TEXT,
    "State" TEXT,
    "ZipCode" TEXT,
    "Budget" DECIMAL(18, 2) NOT NULL,
    "PreferredLocation" TEXT,
    "BedroomsRequired" INTEGER NOT NULL,
    "BathroomsRequired" INTEGER NOT NULL,
    "IsPremium" BOOLEAN NOT NULL DEFAULT FALSE,
    "Notes" TEXT,
    "Status" INTEGER NOT NULL DEFAULT 300,
    "RegistrationDate" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UserId" TEXT,
    "CreatedBy" TEXT,
    "CreatedOn" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastModifiedBy" TEXT,
    "LastModifiedOn" TIMESTAMP,
    "DeletedOn" TIMESTAMP,
    "DeletedBy" TEXT
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Buyers_Email" ON public."Buyers" ("Email");
