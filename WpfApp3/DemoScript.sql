create table units (
	units_id serial4 primary key,
	units_name varchar(50) not null
);

create table supplier (
	supplier_id serial4 primary key,
	supplier_name varchar(50) not null
);

create table manufacturer (
	manufacturer_id serial4 primary key,
	manufacturer_name varchar(50) not null
);

create table category (
	category_id serial4 primary key,
	category_name varchar(50) not null
);

create table products (
	products_id serial4 primary key,
	article varchar(20) not null,
	product_name varchar(50) not null,
	units_id integer not null references units(units_id),
	price integer not null check (price > 0),
	supplier_id integer not null references supplier(supplier_id),
	manufacturer_id	integer not null references manufacturer(manufacturer_id),
	category_id	integer not null references category(category_id),
	discount integer not null check (discount > 0),
	quantity integer not null check (quantity > 0),
	description varchar(200) not null,
	photo bytea,
	photo_path varchar(250)
);

create table role (
	role_id serial4 primary key,
	role_name varchar(50) not null
);

create table users (
	users_id serial4 primary key,
	role_id integer not null references role(role_id),
	surname varchar(50) not null,
	name varchar(50) not null,
	middle_name varchar(50) not null,
	login varchar(50) not null,
	password varchar(50) not null
);

create table pickup_point (
	pickup_point_id serial4 primary key,
	postal_code varchar(10) not null,
	city varchar(50) not null,
	street varchar(50) not null,
	building varchar(50) not null
);

create table status (
	status_id serial4 primary key,
	status_name varchar(50) not null
);

create table orders (
	orders_id serial4 primary key,
	order_date date not null,
	delivery_date date not null,
	pickup_point_id integer not null references pickup_point(pickup_point_id),
	users_id integer not null references users(users_id),
	code varchar(3) not null,
	status_id integer not null references status(status_id)
);

 create table orders_item (
 	orders_item_id serial4 primary key,
 	products_id integer not null references products(products_id),
 	orders_id integer not null references orders(orders_id),
	quantity integer not null check (quantity >= 0)
 );

insert into units(units_name)
values ("шт.");
insert into supplier(supplier_name)
values ("Kari");
insert into manufacturer(manufacturer_name)
values ("Kari"), ("Marco Tozzi"), ("Рос"), ("Rieker"), ("Alessio Nesca"), ("CROSBY");
insert into role(role_name)
values ("Администратор"), ("Менеджер"), ("Авторизованный пользователь");
insert into status(status_name)
values ("Завершен"), ("Новый");