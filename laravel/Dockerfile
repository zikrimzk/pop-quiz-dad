FROM php:8.1-fpm

WORKDIR /var/www

# Install PHP extensions
RUN apt-get update && apt-get install -y \
    git curl zip unzip libzip-dev libpng-dev libjpeg-dev libfreetype6-dev \
    libonig-dev libxml2-dev libssl-dev \
    && docker-php-ext-install pdo pdo_mysql mbstring bcmath zip gd

# Install Composer
COPY --from=composer:2 /usr/bin/composer /usr/bin/composer

# Copy Laravel source code
COPY . .

# Ensure correct permissions
RUN chown -R www-data:www-data /var/www \
    && chmod -R 755 /var/www

# Run Composer install
RUN composer install --no-interaction --prefer-dist --optimize-autoloader
