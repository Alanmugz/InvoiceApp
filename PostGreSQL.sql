--
-- PostgreSQL database dump
--

-- Dumped from database version 9.4.0
-- Dumped by pg_dump version 9.4.0
-- Started on 2015-02-09 16:03:07

SET statement_timeout = 0;
SET lock_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;

SET search_path = public, pg_catalog;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 173 (class 1259 OID 16437)
-- Name: SaleCurrrency; Type: TABLE; Schema: public; Owner: postgres; Tablespace: 
--

CREATE TABLE "SaleCurrrency" (
    "CurrencyId" integer NOT NULL,
    "CurrencyCode" character(3)
);


ALTER TABLE "SaleCurrrency" OWNER TO postgres;

--
-- TOC entry 1992 (class 0 OID 16437)
-- Dependencies: 173
-- Data for Name: SaleCurrrency; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY "SaleCurrrency" ("CurrencyId", "CurrencyCode") FROM stdin;
36	AUD
124	CAD
554	NZD
826	GBP
840	USD
978	EUR
48	BHD
50	BDT
144	LKR
156	CNY
203	DKK
344	HKD
356	INR
392	JYP
398	KZT
400	JOD
404	KES
410	KRW
414	KWD
458	MYR
480	MUR
504	MAD
524	NPR
512	OMR
578	NOK
608	PHP
634	QAR
682	SAR
702	SGD
710	ZAR
752	SEK
756	CHF
760	SYP
764	THB
784	AED
986	BRZ
901	TWD
949	TRY
\.


--
-- TOC entry 1882 (class 2606 OID 16441)
-- Name: Primary_Key_Currency; Type: CONSTRAINT; Schema: public; Owner: postgres; Tablespace: 
--

ALTER TABLE ONLY "SaleCurrrency"
    ADD CONSTRAINT "Primary_Key_Currency" PRIMARY KEY ("CurrencyId");


-- Completed on 2015-02-09 16:03:08

--
-- PostgreSQL database dump complete
--





CREATE TABLE "Transaction"
(
  "TransactionId" integer NOT NULL,
  "MerchantId" integer,
  "MessageTypeId" integer,
  "SaleCurrencyId" integer,
  "SaleValue" double precision,
  "SaleMarginValue" double precision,
  "PaymentCurrencyId" integer,
  "PaymentValue" double precision,
  "PaymentMarginValue" double precision,
  "CreationTimestamp" timestamp without time zone,
  CONSTRAINT "Primary_Key_Transaction" PRIMARY KEY ("TransactionId"),
  CONSTRAINT "Foreign_Key_Currency" FOREIGN KEY ("SaleCurrencyId")
      REFERENCES "SaleCurrrency" ("CurrencyId") MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE NO ACTION
)
WITH (
  OIDS=FALSE
);
ALTER TABLE "Transaction"
  OWNER TO postgres;

-- Index: "fki_Foreign_Key_Currency"

-- DROP INDEX "fki_Foreign_Key_Currency";

CREATE INDEX "fki_Foreign_Key_Currency"
  ON "Transaction"
  USING btree
  ("SaleCurrencyId");
  

