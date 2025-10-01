import en from './en';
import th from './th';

export type Language = 'en' | 'th';

const translations = {
  en,
  th,
};

let currentLanguage: Language = 'th';

export const setLanguage = (language: Language) => {
  currentLanguage = language;
};

export const getLanguage = (): Language => {
  return currentLanguage;
};

export const t = (key: string): string => {
  const keys = key.split('.');
  let value: any = translations[currentLanguage];
  
  for (const k of keys) {
    value = value?.[k];
  }
  
  return value || key;
};

export default translations;
