
enum DirectionEnum {
  asc,
  desc
}

interface ISort {
  property: string;

  direction: DirectionEnum;
}

export { ISort, DirectionEnum } 
