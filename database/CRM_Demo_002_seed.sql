-- --- Usuario administrador (contexto "nuevo") -------------------------------
INSERT INTO usuarios
    (correo, password_hash, nombre, apellido, rol, activo)
VALUES
    ('admin@fullstack-demo.com',
     '$2a$11$AysoFMLGTeIl9nt6g7opJev3HjfqIwmrtilZxKdqLBAjUq47HNcFW',
     'Admin', 'Demo', 'SUPERADMIN', true);

-- --- Tecnico de soporte (contexto legacy/dbo) ----------------------------
INSERT INTO auth_usuarios
    (correo, password_hash, nombre, apellido, telefono, rol, activo, transmision_habilitada)
VALUES
    ('soporte@fullstack-demo.com',
     '$2a$11$AysoFMLGTeIl9nt6g7opJev3HjfqIwmrtilZxKdqLBAjUq47HNcFW',
     'Soporte', 'Demo', 5512345678, 'SOPORTE', true, 'AUTOMATICO');

-- --- Clientes demo (ambos contextos) ---------------------------------------
INSERT INTO clientes
    (legacy_client_id, nombre_comercial, nombre, apellido, rfc, telefono, email, activo, direccion_json)
VALUES
    (1001, 'Cliente Demo SA de CV', 'Cliente', 'Demo Uno', 'DEM010101XX0', '5511112233', 'contacto@clientedemo.com', true,
     '{"calle":"Calle Falsa","numero":"123","ciudad":"Ciudad de Mexico","cp":"01000"}');

INSERT INTO catalog_clientes
    (legacy_client_id, nombre_comercial, nombre, apellido, rfc, telefono, email, activo, direccion_json)
VALUES
    (1001, 'Cliente Demo SA de CV', 'Cliente', 'Demo Uno', 'DEM010101XX0', '5511112233', 'contacto@clientedemo.com', true,
     '{"calle":"Calle Falsa","numero":"123","ciudad":"Ciudad de Mexico","cp":"01000"}');

-- --- Vehiculos demo (ambas flotas) ---------------------------------------
INSERT INTO fleet_vehiculos
    (tipo_vehiculo, transmision, es_de_empresa, placas, activo, nombre_vehiculo, kilometraje_actual)
VALUES
    ('AUTO', 'AUTOMATICO', true, 'DEM-001', true, 'Demo Auto 001', 12500.00);

INSERT INTO vehiculos
    (tipo_vehiculo, transmision, es_de_empresa, placas, activo, nombre_vehiculo, kilometraje)
VALUES
    ('AUTO', 'AUTOMATICO', true, 'DEM-001', true, 'Demo Auto 001', 12500);

-- --- Orden de trabajo demo (ambos contextos) ---------------------------
INSERT INTO ops_ordenes_trabajo
    (cliente_id, creado_por_user_id, asignada_a_user_id, modalidad, tipo_orden, prioridad,
     estado, requiere_factura, costo_estimado, nombre_cliente)
VALUES
    ((SELECT id FROM catalog_clientes WHERE legacy_client_id = 1001),
     (SELECT id FROM auth_usuarios WHERE correo = 'soporte@demo.local'),
     (SELECT id FROM auth_usuarios WHERE correo = 'soporte@demo.local'),
     'PRESENCIAL', 'REPARACION', 2, 'ASIGNADA', false, 1500.00, 'Cliente Demo SA de CV');

INSERT INTO ordenes_trabajo
    (nuevo_cliente, nombre_cliente, cliente_id, creado_por_user_id, modalidad, tipo_orden, prioridad,
     estado, requiere_factura, costo_estimado)
VALUES
    (false, 'Cliente Demo SA de CV',
     (SELECT id FROM clientes WHERE legacy_client_id = 1001),
     (SELECT id FROM auth_usuarios WHERE correo = 'soporte@demo.local'),
     'PRESENCIAL', 'REPARACION', 2, 'ASIGNADA', false, 1500.00);

-- --- Catalogo de productos/servicios demo ---------------------------------
INSERT INTO productos_servicio_ref (nombre, tipo, unidad, precio_lista, legacy_product_id)
VALUES ('Diagnostico general', 'SERVICIO', 'SERVICIO', 350.00, 5001);

INSERT INTO catalog_productos_servicio_ref (nombre, tipo, unidad, precio_lista, legacy_product_id)
VALUES ('Diagnostico general', 'SERVICIO', 'SERVICIO', 350.00, 5001);
