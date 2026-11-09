CREATE DATABASE IF NOT EXISTS gestion_clientes_semana3
    CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE gestion_clientes_semana3;

CREATE TABLE IF NOT EXISTS clientes (
    id INT NOT NULL AUTO_INCREMENT,
    nombres VARCHAR(80) NOT NULL,
    apellidos VARCHAR(80) NOT NULL,
    identificacion VARCHAR(20) NOT NULL,
    correo VARCHAR(150) NOT NULL,
    telefono VARCHAR(20) NOT NULL,
    direccion VARCHAR(160) NOT NULL,
    ciudad VARCHAR(80) NOT NULL,
    provincia VARCHAR(80) NOT NULL,
    fecha_nacimiento DATE NOT NULL,
    estado ENUM('Activo','Inactivo') NOT NULL DEFAULT 'Activo',
    PRIMARY KEY (id),
    UNIQUE KEY uq_clientes_identificacion (identificacion)
);
