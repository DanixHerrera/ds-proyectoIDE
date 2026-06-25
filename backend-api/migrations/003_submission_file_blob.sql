-- Add file BLOB columns to entregas table for solution file submissions
SET @s = (SELECT IF(
  (SELECT COUNT(*) FROM information_schema.columns
   WHERE table_schema = DATABASE() AND table_name = 'entregas' AND column_name = 'nombre_archivo') = 0,
  'ALTER TABLE entregas
   ADD COLUMN nombre_archivo VARCHAR(255) DEFAULT NULL AFTER numero_intento,
   ADD COLUMN tipo_mime VARCHAR(127) DEFAULT NULL AFTER nombre_archivo,
   ADD COLUMN tamano BIGINT DEFAULT NULL AFTER tipo_mime,
   ADD COLUMN datos MEDIUMBLOB DEFAULT NULL AFTER tamano',
  'SELECT 1'
));
PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
