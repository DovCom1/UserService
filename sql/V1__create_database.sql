CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    uid VARCHAR(10) NOT NULL UNIQUE,
    nickname VARCHAR(50) NOT NULL,
    email VARCHAR(128) NOT NULL UNIQUE,
    avatar_url VARCHAR(255) DEFAULT "",
    gender SMALLINT NOT NULL CHECK (gender in (1, 2)), -- 1=Мужской, 2=Женский
    status SMALLINT NOT NULL CHECK (status in (1, 2, 3, 4)), -- 1=В сети, 2=Не активен, 3=Не беспокоить, 4=Не в сети
    date_of_birth DATE NOT NULL,
    account_creation_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_users_nickname ON users(nickname);
CREATE INDEX idx_users_nickname_uid ON users(nickname, uid);

CREATE TABLE IF NOT EXISTS friends (
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    friend_id UUID REFERENCES users(id) ON DELETE CASCADE,
    status SMALLINT NOT NULL DEFAULT 1 CHECK (status in (1, 2)), -- 1=Заявка отправлена, 2=Друг
    PRIMARY KEY (user_id, friend_id),
    CHECK (user_id != friend_id)
);

CREATE INDEX idx_friends_user_id_status ON friends(user_id, status);
CREATE INDEX idx_friends_friend_id_status ON friends(friend_id, status);

CREATE TABLE IF NOT EXISTS enemies (
    user_id UUID REFERENCES users(id) ON DELETE CASCADE,
    enemy_id UUID REFERENCES users(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, enemy_id),
    CHECK (user_id != enemy_id)
);

CREATE OR REPLACE FUNCTION prevent_account_creation_time_update() RETURNS TRIGGER AS $$
BEGIN
    IF NEW.account_creation_time IS DISTINCT FROM OLD.account_creation_time THEN
        RAISE EXCEPTION 'It is forbidden to change the date of creation of the account.';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER protect_account_creation_time
BEFORE UPDATE OF account_creation_time ON users
FOR EACH ROW EXECUTE FUNCTION prevent_account_creation_time_update();