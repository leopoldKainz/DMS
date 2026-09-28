CREATE TABLE IF NOT EXISTS "users"(
    "id" SERIAL PRIMARY KEY,
    "entity_id" UUID NOT NULL UNIQUE,
    "username" VARCHAR(20) NOT NULL UNIQUE,
    "email" VARCHAR(50) NOT NULL UNIQUE,
    "password" VARCHAR(100) NOT NULL,
    "created_at" TIMESTAMP(0) WITH TIME ZONE NOT NULL,
    "updated_on" TIMESTAMP(0) WITH TIME ZONE NULL
);

CREATE TABLE IF NOT EXISTS "documents"(
    "id" SERIAL PRIMARY KEY,
    "entity_id" UUID NOT NULL UNIQUE,
    "name" VARCHAR(255) NOT NULL,
    "type" VARCHAR(100) NOT NULL,
    "size" BIGINT NOT NULL,
    "created_at" TIMESTAMP(0) WITH TIME ZONE NOT NULL,
    "updated_on" TIMESTAMP(0) WITH TIME ZONE NULL
);

CREATE TABLE IF NOT EXISTS "user_documents"(
    "user_id" INTEGER NOT NULL REFERENCES "users"("id") ON DELETE CASCADE,
    "document_id" INTEGER NOT NULL REFERENCES "documents"("id") ON DELETE CASCADE,
    "permission" VARCHAR(20) NOT NULL,
    "created_at" TIMESTAMP(0) WITH TIME ZONE NOT NULL,
    "updated_on" TIMESTAMP(0) WITH TIME ZONE NULL,
    PRIMARY KEY ("user_id", "document_id")
);

CREATE TABLE IF NOT EXISTS "share_requests"(
    "id" SERIAL PRIMARY KEY,
    "entity_id" UUID NOT NULL UNIQUE,
    "owner_id" INTEGER NOT NULL REFERENCES "users"("id") ON DELETE CASCADE,
    "reciever_id" INTEGER NOT NULL REFERENCES "users"("id") ON DELETE RESTRICT,
    "document_id" INTEGER NOT NULL REFERENCES "documents"("id") ON DELETE CASCADE,
    "created_at" TIMESTAMP(0) WITH TIME ZONE NOT NULL,
    "updated_on" TIMESTAMP(0) WITH TIME ZONE NULL,
    CONSTRAINT "share_requests_owner_id_reciever_id_document_id_unique" UNIQUE ("owner_id", "reciever_id", "document_id")
);
