-- Script para verificar usuarios existentes
USE cabs_pruebas;
SELECT TOP 5 id, email FROM auth_usuarios ORDER BY id;
