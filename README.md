<div align="center">

# ALBION KILLBOARD BOT C#

</div>

Самая простая реализация KILLBOARD BOT, для Albion Online.
Ставится на Linux/Windows сервера. Без огромных требований к железу.
Реализована система фильтров, и возможности продажи "подписок" на бот + триал версию на 50 бесплатных уведомлений.

# Как запускать

1. Регаем Бота на сайте https://discord.com/developers/applications/
2. Получаем Токен для бота, заменяем его в файле Program.cs
3. На VPS/VDS (Linux/Windows) устанавливаем NET 7.0
4. Билдим проект
5. Linux - cd в папку проекта, dotnet BOT.dll / Windows - запускаете .exe файл
6. Бот запущен

# Команды бота описаны в файле AdminModule

1. /subscription -> Статус подписки на бота
2. /bindchannel -> Привязать канал к "сообщениям"
3. /unbind -> Убрать "сообщения" с канала
4. /mastervoice -> Создаёт канал для "приватных" комнат
5. /premium -> Активирует премиум статус для сервера (снимает лимит)

# Наш Discord

- Link https://discord.gg/8byNr7TDma
