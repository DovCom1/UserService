CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    uid VARCHAR(10) NOT NULL UNIQUE,
    nickname VARCHAR(50) NOT NULL,
    email VARCHAR(128) NOT NULL UNIQUE,
    avatar_url VARCHAR(255) DEFAULT "",
    gender SMALLINT NOT NULL CHECK (gender in (1, 2)), -- 1=Мужской, 2=Женский
    status SMALLINT NOT NULL CHECK (status in (1, 2, 3, 4)), -- 1=В сети, 2=Не активен, 3=Не беспокоить, 4=Не в сети
    date_of_birth DATE NOT NULL,
    account_creation_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_users_nickname ON users(nickname);
CREATE INDEX idx_users_nickname_uid ON users(nickname, uid);

CREATE TABLE IF NOT EXISTS friends (
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    friend_id UUID REFERENCES users(id) ON DELETE CASCADE,
    status SMALLINT NOT NULL DEFAULT 1 CHECK (status in (1, 2, 3)), -- 1=Заявка отправлена, 2=Друг, 3=Заявка отклонена 
    PRIMARY KEY (user_id, friend_id),
    CHECK (user_id != friend_id)
);

CREATE INDEX idx_friends_status ON friends(status);
CREATE INDEX idx_friends_user_id_status ON friends(user_id, status);

CREATE TABLE IF NOT EXISTS enemies (
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    enemy_id UUID REFERENCES users(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, enemy_id),
    CHECK (user_id != enemy_id)
);