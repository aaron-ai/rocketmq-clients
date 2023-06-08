from apscheduler.schedulers.background import BackgroundScheduler

from rocketmq.log import logger
from apscheduler.schedulers.blocking import BlockingScheduler
from apscheduler.executors.pool import ThreadPoolExecutor, ProcessPoolExecutor
import time


def task():
    logger.info("Hello World")


if __name__ == "__main__":
    executors = {
        'default': ThreadPoolExecutor(20),
        'processpool': ProcessPoolExecutor(5)
    }

    scheduler = BackgroundScheduler(executors = executors)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)
    scheduler.add_job(task, 'interval', seconds=1)



    scheduler.start()
    time.sleep(30)
    scheduler.shutdown()
    logger.info("===============")
