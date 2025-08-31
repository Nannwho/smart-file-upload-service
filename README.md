# Smart File Upload Service

Умный сервис для загрузки файлов с проверкой на вирусы, написанный на ASP.NET Core.

## 🚀 Возможности

- Загрузка файлов через REST API
- Проверка на вирусы с помощью ClamAV
- Валидация типа и размера файлов
- Автоматическое перемещение файлов в карантин при обнаружении угроз
- Swagger UI для тестирования API
- Health checks для мониторинга состояния сервиса

## 🛠 Технологии

- ASP.NET Core 8.0
- nClam (клиент для ClamAV)
- Swagger/OpenAPI
- Встроенная система Health Checks

## 📦 Установка и запуск

1. Клонируйте репозиторий:
```bash
git clone https://github.com/your-username/smart-file-upload-service.git
