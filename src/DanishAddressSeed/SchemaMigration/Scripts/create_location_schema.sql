CREATE SCHEMA IF NOT EXISTS "location"
    AUTHORIZATION postgres;

CREATE TABLE IF NOT EXISTS "location"."official_access_address" (
    "id" uuid PRIMARY KEY,
    "coord" geometry(Point,25832),
	"status" varchar(50), -- active, canceled, pending, discontinued
	"house_number" varchar(50),
	"road_code" varchar(50),
	"road_name" varchar(255),
	"town_name" varchar(255),
	"post_district_code" varchar(50),
	"post_district_name" varchar(255),
	"municipal_code" varchar(50),
    "access_address_external_id" varchar(255),
	"road_external_id" varchar(255),
	"plot_external_id" varchar(255),
	"created" timestamptz,
	"updated" timestamptz,
	"location_updated" timestamptz
);

CREATE INDEX IF NOT EXISTS ix_official_access_address_coord
    ON "location"."official_access_address" USING gist ("coord");

CREATE TABLE IF NOT EXISTS "location"."official_unit_address" (
    "id" uuid PRIMARY KEY,
	"access_address_id" uuid,
    "status" varchar(50),
	"floor_name" varchar(80),
	"suit_name" varchar(80),
	"unit_address_external_id" varchar(255),
	"access_address_external_id" varchar(255),
	"created" timestamptz,
	"updated" timestamptz
);
