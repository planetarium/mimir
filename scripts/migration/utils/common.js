/**
 * 지정된 시간(밀리초) 동안 실행을 일시 중지합니다.
 * @param {number} ms - 지연 시간(밀리초)
 * @returns {Promise<void>}
 */
const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));

/**
 * 특정 시간 간격으로 진행 상황을 업데이트하는 진행률 표시기를 생성합니다.
 * @param {number} total - 전체 항목 수
 * @param {number} updateIntervalMs - 업데이트 간격(밀리초)
 * @returns {Object} - 진행률 표시기 객체
 */
const createProgressTracker = (total, updateIntervalMs = 5000) => {
  let processed = 0;
  let lastUpdateTime = Date.now();
  let startTime = Date.now();
  
  return {
    increment: () => {
      processed++;
      const now = Date.now();
      
      if (now - lastUpdateTime >= updateIntervalMs || processed === total) {
        const elapsedSeconds = (now - startTime) / 1000;
        const itemsPerSecond = processed / elapsedSeconds;
        const percent = (processed / total * 100).toFixed(2);
        const remaining = total - processed;
        const estimatedRemainingTime = remaining / itemsPerSecond;
        
        console.log(`진행률: ${processed}/${total} (${percent}%), ` +
                    `처리 속도: ${itemsPerSecond.toFixed(2)}개/초, ` +
                    `남은 시간: ${formatTime(estimatedRemainingTime)}`);
        
        lastUpdateTime = now;
      }
    },
    complete: () => {
      const totalTime = (Date.now() - startTime) / 1000;
      console.log(`완료! 총 처리: ${processed}개, 소요 시간: ${formatTime(totalTime)}`);
    }
  };
};

/**
 * 초 단위 시간을 시:분:초 형식으로 변환합니다.
 * @param {number} seconds - 초 단위 시간
 * @returns {string} - 형식화된 시간 문자열
 */
const formatTime = (seconds) => {
  const hrs = Math.floor(seconds / 3600);
  const mins = Math.floor((seconds % 3600) / 60);
  const secs = Math.floor(seconds % 60);
  
  return `${hrs}시간 ${mins}분 ${secs}초`;
};

/**
 * 배열을 지정된 크기의 청크로 분할합니다.
 * @param {Array} array - 분할할 배열
 * @param {number} chunkSize - 청크 크기
 * @returns {Array} - 청크 배열
 */
const chunkArray = (array, chunkSize) => {
  const chunks = [];
  for (let i = 0; i < array.length; i += chunkSize) {
    chunks.push(array.slice(i, i + chunkSize));
  }
  return chunks;
};

module.exports = {
  sleep,
  createProgressTracker,
  formatTime,
  chunkArray
}; 